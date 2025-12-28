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

    public void ProduceFirstUnit()
    {
        if (!model.HasSelection)
        {
            return;
        }

        if (onStartProduceUnit != null)
        {
            onStartProduceUnit.Raise(new UnitProducingData(model.SelectedConstructionId, model.FirstUnitId));
        }
    }

    private void OnSelectedConstruction(UnitProduceConstructionConfig constructionConfig)
    {
        if (constructionConfig == null)
        {
            model.SelectedConstructionId = 0;
            model.FirstUnitId = 0;
            model.HasSelection = false;
            view.SetProduceButtonVisible(false);
            return;
        }

        model.SelectedConstructionId = constructionConfig.ConstructionId;
        model.FirstUnitId = constructionConfig.UnitProducible[0];
        model.HasSelection = true;
        view.SetProduceButtonVisible(true);
    }

    private void OnDeselectedConstruction(int constructionId)
    {
        model.SelectedConstructionId = 0;
        model.FirstUnitId = 0;
        model.HasSelection = false;

        if (view != null)
        {
            view.SetProduceButtonVisible(false);
        }
    }
}

