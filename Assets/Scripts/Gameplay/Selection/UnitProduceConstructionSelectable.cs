using UnityEngine;
using UnityEngine.Serialization;

public class UnitProduceConstructionSelectable : SelectableObject
{
    [SerializeField] private UnitProduceConstructionConfig constructionConfig;
    [SerializeField] private UnitProduceConstructionConfigEventChannelSO onSelectedUnitProduceConstruction;
    [SerializeField] private GameObject selectionIndicator;
    [SerializeField] private IntEventChannelSO onDeselectedConstruction;

    
    private void OnEnable()
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
    }

    public override void OnSelected()
    {
        base.OnSelected();

        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(true);
        }

        if (onSelectedUnitProduceConstruction != null)
        {
            onSelectedUnitProduceConstruction.Raise(constructionConfig);
        }
    }

    public override void OnDeselected()
    {
        base.OnDeselected();

        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
        
        if (onDeselectedConstruction != null)
        {
            onDeselectedConstruction.Raise(constructionConfig.ConstructionId);
        }
    }
}
