using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
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
    private readonly List<CommandExecutor> buildExecutors = new List<CommandExecutor>();
    private readonly HashSet<CommandExecutor> buildExecutorSet = new HashSet<CommandExecutor>();
    private Coroutine invalidPlacementFlashRoutine;

    private static readonly int baseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int colorId = Shader.PropertyToID("_Color");

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
            PlaceConstruction();
        }
    }

    private void OnSelectedBuildCapableUnit(GameObject unitObject)
    {
        if (unitObject == null)
        {
            return;
        }

        if (unitObject.TryGetComponent(out CommandExecutor executor) && executor != null && buildExecutorSet.Add(executor))
        {
            buildExecutors.Add(executor);
        }
    }

    private void OnDeselectedBuildCapableUnit()
    {
        buildExecutorSet.Clear();
        buildExecutors.Clear();
        CancelBuildMode();
    }

    private void OnBuildConstructionSelected(int buildingId)
    {
        if (buildExecutors.Count == 0)
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
            IBuildable construction = GetConstruction(previewInstance);
            if (construction == null)
            {
                Addressables.ReleaseInstance(previewInstance);
                return;
            }
            construction.Preview();
            UpdatePreviewPosition();
        };
    }

    public void CancelBuildMode()
    {
        if (!isBuildMode)
            return;
        isBuildMode = false;
        selectedBuildingId = 0;

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

    private void PlaceConstruction()
    {
        if (previewInstance == null)
        {
            return;
        }

        if (!CanPlace(previewInstance))
        {
            Debug.LogError("Cannot place here");
            // ShowInvalidPlacementFx(previewInstance);
            return;
        }

        GameObject placed = previewInstance;
        previewInstance = null;
        isBuildMode = false;
        selectedBuildingId = 0;

        if (buildExecutors.Count == 0)
        {
            Addressables.ReleaseInstance(placed);
            return;
        }

        IBuildable construction = GetConstruction(placed);
        if (construction == null)
        {
            Addressables.ReleaseInstance(placed);
            return;
        }

        construction.Place();
        for (int i = buildExecutors.Count - 1; i >= 0; i--)
        {
            CommandExecutor executor = buildExecutors[i];
            if (executor == null)
            {
                buildExecutorSet.Remove(executor);
                buildExecutors.RemoveAt(i);
                continue;
            }

            executor.SetCommand(new BuildCommand(construction));
        }
    }

    private static bool CanPlace(GameObject construction)
    {
        if (construction == null)
        {
            return false;
        }

        if (GridManager.Instance == null)
        {
            return true;
        }

        IGridEntity gridEntity = construction.GetComponent<IGridEntity>();
        Vector2Int size = gridEntity != null ? gridEntity.GetSize() : Vector2Int.one;
        Vector3 worldPosition = construction.transform.position;

        int width = Mathf.Max(1, size.x);
        int height = Mathf.Max(1, size.y);

        Vector2Int origin = GridManager.Instance.WorldToGrid(worldPosition);
        int left = (width - 1) / 2;
        int right = width / 2;
        int bottom = (height - 1) / 2;
        int top = height / 2;

        for (int x = origin.x - left; x <= origin.x + right; x++)
        {
            for (int z = origin.y - bottom; z <= origin.y + top; z++)
            {
                Vector2Int cell = new Vector2Int(x, z);
                if (!GridManager.Instance.IsCellFree(cell))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void ShowInvalidPlacementFx(GameObject previewRoot)
    {
        if (invalidPlacementFlashRoutine != null)
        {
            StopCoroutine(invalidPlacementFlashRoutine);
            invalidPlacementFlashRoutine = null;
        }

        invalidPlacementFlashRoutine = StartCoroutine(IEShowInvalidPlacementFxRoutine(previewRoot));
    }

    private System.Collections.IEnumerator IEShowInvalidPlacementFxRoutine(GameObject previewRoot)
    {
        if (previewRoot == null)
        {
            yield break;
        }

        Renderer[] renderers = previewRoot.GetComponentsInChildren<Renderer>(true);
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null)
            {
                continue;
            }

            r.GetPropertyBlock(block);
            block.SetColor(baseColorId, Color.red);
            block.SetColor(colorId, Color.red);
            r.SetPropertyBlock(block);
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null)
            {
                continue;
            }

            r.SetPropertyBlock(null);
        }

        invalidPlacementFlashRoutine = null;
    }

    private static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private static IBuildable GetConstruction(GameObject root)
    {
        if (root == null)
        {
            return null;
        }

        return root.GetComponentInChildren<IBuildable>(true);
    }
}
