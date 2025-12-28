using UnityEngine;
using UnityEngine.Serialization;

public class ConstructionInteractionUI : MonoBehaviour
{
    [SerializeField] private IntEventChannelSO onDeselectedConstruction;
    [SerializeField] private UnitProduceConstructionConfigEventChannelSO onSelectedUnitProduceConstruction;

    [SerializeField] private UnitProducingEventChannelSO onStartProduceUnit;
    [SerializeField] private GameObject buttonProduceUnit;

    private int selectedConstructionId;
    private int firstUnitId;

    private void OnEnable()
    {
        onDeselectedConstruction.Register(OnDeselectConstruction);
        onSelectedUnitProduceConstruction.Register(OnSelectConstruction);
        buttonProduceUnit.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        onDeselectedConstruction.Unregister(OnDeselectConstruction);
        onSelectedUnitProduceConstruction.Unregister(OnSelectConstruction);
    }

    private void OnSelectConstruction(UnitProduceConstructionConfig constructionConfig)
    {
        selectedConstructionId = constructionConfig != null ? constructionConfig.ConstructionId : 0;
        firstUnitId = constructionConfig.UnitProducible[0];
        buttonProduceUnit.gameObject.SetActive(true);
    }
    
    private void OnDeselectConstruction(int constructionId)
    {
        selectedConstructionId = 0;
        buttonProduceUnit.gameObject.SetActive(false);
    }

    public void ClickProduceUnit()
    {
        onStartProduceUnit.Raise(new UnitProducingData(selectedConstructionId, firstUnitId));
    }
}
