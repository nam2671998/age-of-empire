using UnityEngine;

public sealed class SettlerActionUIController : MonoBehaviour
{
    [SerializeField] private GameObjectEventChannelSO onSelectedSettler;
    [SerializeField] private VoidEventChannelSO onDeselectedSettler;
    [SerializeField] private VoidEventChannelSO onOpenBuildConstructionUI;
    [SerializeField] private VoidEventChannelSO onCloseBuildConstructionUI;

    private SettlerActionUIView view;
    private SettlerActionUIModel model;

    private void Awake()
    {
        model = new SettlerActionUIModel();
    }

    public void Initialize(SettlerActionUIView view)
    {
        this.view = view;
    }

    private void OnEnable()
    {
        if (onSelectedSettler != null)
        {
            onSelectedSettler.Register(OnSelectedSettler);
        }

        if (onDeselectedSettler != null)
        {
            onDeselectedSettler.Register(OnDeselectedSettler);
        }

        if (view != null)
        {
            view.SetPanelVisible(false);
        }
    }

    private void OnDisable()
    {
        if (onSelectedSettler != null)
        {
            onSelectedSettler.Unregister(OnSelectedSettler);
        }

        if (onDeselectedSettler != null)
        {
            onDeselectedSettler.Unregister(OnDeselectedSettler);
        }

        if (onCloseBuildConstructionUI != null)
        {
            onCloseBuildConstructionUI.Raise();
        }
    }

    public void RequestOpenBuildConstructionUI()
    {
        if (model.SelectedExecutor == null)
        {
            return;
        }

        if (onOpenBuildConstructionUI != null)
        {
            onOpenBuildConstructionUI.Raise();
        }
    }

    public void AbortCurrentCommand()
    {
        if (model.SelectedExecutor == null)
        {
            return;
        }

        model.SelectedExecutor.ClearCommands();

        if (model.SelectedExecutor.TryGetComponent(out IStopAction stopAction))
        {
            stopAction.StopOtherActions();
        }

        if (model.SelectedExecutor.TryGetComponent(out UnitActionStateController stateController))
        {
            stateController.ResetToIdle();
        }
    }

    private void OnSelectedSettler(GameObject unitObject)
    {
        if (unitObject == null)
        {
            model.SelectedExecutor = null;
            return;
        }

        unitObject.TryGetComponent(out CommandExecutor modelSelectedExecutor);
        view.SetPanelVisible(modelSelectedExecutor != null);
    }

    private void OnDeselectedSettler()
    {
        model.SelectedExecutor = null;

        if (onCloseBuildConstructionUI != null)
        {
            onCloseBuildConstructionUI.Raise();
        }

        if (view != null)
        {
            view.SetPanelVisible(false);
        }
    }
}

