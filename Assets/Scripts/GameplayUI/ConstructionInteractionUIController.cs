using UnityEngine;

public sealed class ConstructionInteractionUIController : MonoBehaviour
{
    [SerializeField] private IntEventChannelSO onDeselectedConstruction;
    [SerializeField] private UnitProduceConstructionConfigEventChannelSO onSelectedUnitProduceConstruction;
    [SerializeField] private UnitProducingEventChannelSO onStartProduceUnit;

    private ConstructionInteractionUIView view;
    private ConstructionInteractionUIModel model;

    private void Awake()
    {
        model = new ConstructionInteractionUIModel();
    }

    public void Initialize(ConstructionInteractionUIView view)
    {
        this.view = view;
    }

    private void OnEnable()
    {
        if (onDeselectedConstruction != null)
        {
            onDeselectedConstruction.Register(OnDeselectedConstruction);
        }

        if (onSelectedUnitProduceConstruction != null)
        {
            onSelectedUnitProduceConstruction.Register(OnSelectedConstruction);
        }

        if (view != null)
        {
            view.SetProduceButtonVisible(false);
        }
    }

    private void OnDisable()
    {
        if (onDeselectedConstruction != null)
        {
            onDeselectedConstruction.Unregister(OnDeselectedConstruction);
        }

        if (onSelectedUnitProduceConstruction != null)
        {
            onSelectedUnitProduceConstruction.Unregister(OnSelectedConstruction);
        }
    }

    public void SelectProduceUnit(int unitId)
    {
        if (!model.HasSelection)
        {
            return;
        }

        if (onStartProduceUnit != null)
        {
            onStartProduceUnit.Raise(new UnitProducingData(model.SelectedConstructionId, unitId));
        }
    }

    private void OnSelectedConstruction(UnitProduceConstructionConfig constructionConfig)
    {
        if (constructionConfig == null)
        {
            model.SelectedConstructionId = 0;
            model.ProducibleUnitIds = null;
            model.HasSelection = false;
            if (view != null)
            {
                view.SetProduceButtonVisible(false);
            }
            return;
        }

        model.SelectedConstructionId = constructionConfig.ConstructionId;
        model.ProducibleUnitIds = constructionConfig.UnitProducible;
        model.HasSelection = true;
        if (view != null)
        {
            view.SetProduceButtonVisible(true);
            view.RefreshProduceOptions(model.ProducibleUnitIds);
        }
    }

    private void OnDeselectedConstruction(int constructionId)
    {
        model.SelectedConstructionId = 0;
        model.ProducibleUnitIds = null;
        model.HasSelection = false;

        if (view != null)
        {
            view.SetProduceButtonVisible(false);
        }
    }
}
