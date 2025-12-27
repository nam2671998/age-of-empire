using System;
using UnityEngine;

public class UnitSelectable : SelectableObject
{
    [SerializeField] private GameObject selectionIndicator;

    private void OnEnable()
    {
        selectionIndicator.SetActive(false);
    }

    public override void OnSelected()
    {
        base.OnSelected();
        selectionIndicator.SetActive(true);
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
        selectionIndicator.SetActive(false);
    }
}
