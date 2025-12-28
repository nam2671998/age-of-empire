using UnityEngine;

public sealed class BuildConstructionUIController : MonoBehaviour
{
    [SerializeField] private GameObjectEventChannelSO onSelectedBuildCapableUnit;
    [SerializeField] private VoidEventChannelSO onDeselectedBuildCapableUnit;
    [SerializeField] private VoidEventChannelSO onOpenBuildConstructionUI;
    [SerializeField] private VoidEventChannelSO onCloseBuildConstructionUI;
    [SerializeField] private IntEventChannelSO onBuildConstructionSelected;
    [SerializeField] private VoidEventChannelSO onCancelBuildModeRequested;

    private BuildConstructionUIView view;
    private BuildConstructionUIModel model;

    private void Awake()
    {
        model = new BuildConstructionUIModel();
    }

    public void Initialize(BuildConstructionUIView view)
    {
        this.view = view;
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

        if (onOpenBuildConstructionUI != null)
        {
            onOpenBuildConstructionUI.Register(OpenRequested);
        }

        if (onCloseBuildConstructionUI != null)
        {
            onCloseBuildConstructionUI.Register(CloseRequested);
        }

        if (view != null)
        {
            view.SetPanelVisible(false);
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

        if (onOpenBuildConstructionUI != null)
        {
            onOpenBuildConstructionUI.Unregister(OpenRequested);
        }

        if (onCloseBuildConstructionUI != null)
        {
            onCloseBuildConstructionUI.Unregister(CloseRequested);
        }

        if (onCancelBuildModeRequested != null)
        {
            onCancelBuildModeRequested.Raise();
        }
    }

    public void SelectBuildOption(int buildingId)
    {
        if (!model.HasSelectedExecutor)
        {
            return;
        }

        if (onBuildConstructionSelected != null)
        {
            onBuildConstructionSelected.Raise(buildingId);
        }
    }

    private void OnSelectedBuildCapableUnit(GameObject unitObject)
    {
        model.HasSelectedExecutor = unitObject != null && unitObject.TryGetComponent(out CommandExecutor _);
    }

    private void OnDeselectedBuildCapableUnit()
    {
        model.HasSelectedExecutor = false;

        if (onCancelBuildModeRequested != null)
        {
            onCancelBuildModeRequested.Raise();
        }

        if (view != null)
        {
            view.Close();
        }
    }

    private void OpenRequested()
    {
        if (view != null)
        {
            view.Open();
        }
    }

    private void CloseRequested()
    {
        if (onCancelBuildModeRequested != null)
        {
            onCancelBuildModeRequested.Raise();
        }

        if (view != null)
        {
            view.Close();
        }
    }
}

