using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BuildModeController : MonoBehaviour
{
    [SerializeField] private GameObjectEventChannelSO onSelectedBuildCapableUnit;
    [SerializeField] private VoidEventChannelSO onDeselectedBuildCapableUnit;
    [SerializeField] private IntEventChannelSO onBuildConstructionSelected;
    [SerializeField] private VoidEventChannelSO onCancelBuildModeRequested;
    [SerializeField] private Camera mainCamera;

    private bool isBuildMode;
    private int selectedBuildingId;
    private GameObject previewInstance;
    private CommandExecutor buildExecutor;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        if (onSelectedBuildCapableUnit != null)
        {
            onSelectedBuildCapableUnit.Register(OnSelectedBuildCapableUnit);
        }

        if (onDeselectedBuildCapableUnit != null)
        {
            onDeselectedBuildCapableUnit.Register(OnDeselectedBuildCapableUnit);
        }

        if (onBuildConstructionSelected != null)
        {
            onBuildConstructionSelected.Register(OnBuildConstructionSelected);
        }

        if (onCancelBuildModeRequested != null)
        {
            onCancelBuildModeRequested.Register(CancelBuildMode);
        }
    }

    private void OnDisable()
    {
        if (onSelectedBuildCapableUnit != null)
        {
            onSelectedBuildCapableUnit.Unregister(OnSelectedBuildCapableUnit);
        }

        if (onDeselectedBuildCapableUnit != null)
        {
            onDeselectedBuildCapableUnit.Unregister(OnDeselectedBuildCapableUnit);
        }

        if (onBuildConstructionSelected != null)
        {
            onBuildConstructionSelected.Unregister(OnBuildConstructionSelected);
        }

        if (onCancelBuildModeRequested != null)
        {
            onCancelBuildModeRequested.Unregister(CancelBuildMode);
        }

        CancelBuildMode();
    }

    private void Update()
    {
        if (!isBuildMode || previewInstance == null)
        {
            return;
        }

        UpdatePreviewPosition();

        if (Input.GetMouseButtonDown(1))
        {
            CancelBuildMode();
            return;
        }

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            PlaceAndCommand();
        }
    }

    private void OnSelectedBuildCapableUnit(GameObject unitObject)
    {
        if (unitObject == null)
        {
            return;
        }

        if (unitObject.TryGetComponent(out CommandExecutor executor))
        {
            buildExecutor = executor;
        }
    }

    private void OnDeselectedBuildCapableUnit()
    {
        buildExecutor = null;
        CancelBuildMode();
    }

    private void OnBuildConstructionSelected(int buildingId)
    {
        if (buildExecutor == null)
        {
            return;
        }

        StartBuildMode(buildingId);
    }

    private void StartBuildMode(int buildingId)
    {
        CancelBuildMode();

        isBuildMode = true;
        selectedBuildingId = buildingId;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        string key = $"Constructions/{buildingId}.prefab";
        Addressables.InstantiateAsync(key).Completed += handle =>
        {
            if (!isBuildMode || selectedBuildingId != buildingId)
            {
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    Addressables.ReleaseInstance(handle.Result);
                }
                return;
            }

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                CancelBuildMode();
                return;
            }

            previewInstance = handle.Result;
            UpdatePreviewPosition();
        };
    }

    public void CancelBuildMode()
    {
        isBuildMode = false;
        selectedBuildingId = 0;
        buildExecutor = null;

        if (previewInstance != null)
        {
            Addressables.ReleaseInstance(previewInstance);
            previewInstance = null;
        }
    }

    private void UpdatePreviewPosition()
    {
        if (previewInstance == null)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (!plane.Raycast(ray, out float distance))
        {
            return;
        }

        Vector3 world = ray.GetPoint(distance);
        Vector3 snapped = new Vector3(Mathf.Round(world.x), 0f, Mathf.Round(world.z));
        previewInstance.transform.position = snapped;
    }

    private void PlaceAndCommand()
    {
        if (previewInstance == null)
        {
            return;
        }

        GameObject placed = previewInstance;
        previewInstance = null;
        isBuildMode = false;
        selectedBuildingId = 0;

        if (buildExecutor == null)
        {
            Addressables.ReleaseInstance(placed);
            return;
        }

        IBuildable buildable = FindBuildable(placed);
        if (buildable == null)
        {
            Addressables.ReleaseInstance(placed);
            return;
        }

        buildExecutor.SetCommand(new BuildCommand(buildable));
        buildExecutor = null;
    }

    private static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private static IBuildable FindBuildable(GameObject root)
    {
        if (root == null)
        {
            return null;
        }

        var behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IBuildable buildable)
            {
                return buildable;
            }
        }

        return null;
    }
}
