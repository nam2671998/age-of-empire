using UnityEngine;

public class UnitSelectable : SelectableObject
{
    [SerializeField] private GameObject selectionIndicator;
    [SerializeField] private GameObjectEventChannelSO onSelectedUnit;
    [SerializeField] private VoidEventChannelSO onDeselectedUnit;

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

        if (onSelectedUnit != null)
        {
            onSelectedUnit.Raise(gameObject);
        }
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }

        if (onDeselectedUnit != null)
        {
            onDeselectedUnit.Raise();
        }
    }
}
