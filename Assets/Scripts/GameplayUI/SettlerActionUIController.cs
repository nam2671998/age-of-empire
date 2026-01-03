using System.Collections.Generic;
using UnityEngine;

public class SettlerActionUIController : MonoBehaviour
{
    [SerializeField] private GameObjectEventChannelSO onSelectedSettler;
    [SerializeField] private VoidEventChannelSO onDeselectedSettler;
    [SerializeField] private VoidEventChannelSO onOpenBuildConstructionUI;
    [SerializeField] private VoidEventChannelSO onCloseBuildConstructionUI;

    private SettlerActionUIView view;
    private SettlerActionUIModel model;
    private HashSet<CommandExecutor> selectionSet;

    private void Awake()
    {
        model = new SettlerActionUIModel();
        selectionSet = new HashSet<CommandExecutor>();
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
        if (model.SelectedExecutors.Count == 0)
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
        if (model.SelectedExecutors.Count == 0)
        {
            return;
        }

        for (int i = 0; i < model.SelectedExecutors.Count; i++)
        {
            CommandExecutor executor = model.SelectedExecutors[i];
            if (executor == null)
                continue;

            executor.ClearCommands();

            if (executor.TryGetComponent(out IStopAction stopAction))
            {
                stopAction.StopOtherActions();
            }

            if (executor.TryGetComponent(out UnitActionStateController stateController))
            {
                stateController.ResetToIdle();
            }
        }
    }

    private void OnSelectedSettler(GameObject unitObject)
    {
        if (unitObject == null)
        {
            selectionSet.Clear();
            model.SelectedExecutors.Clear();
            return;
        }

        if (!unitObject.TryGetComponent(out CommandExecutor executor) || executor == null)
        {
            return;
        }

        if (selectionSet.Add(executor))
        {
            model.SelectedExecutors.Add(executor);
        }

        if (view != null)
        {
            view.SetPanelVisible(model.SelectedExecutors.Count > 0);
        }
    }

    private void OnDeselectedSettler()
    {
        selectionSet.Clear();
        model.SelectedExecutors.Clear();

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
