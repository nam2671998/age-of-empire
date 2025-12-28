using UnityEngine;

/// <summary>
/// Component that implements IGameSelectable for easy attachment to GameObjects
/// </summary>
public class SelectableObject : MonoBehaviour, IGameSelectable
{
    [SerializeField] private SelectableSelectionMode selectionMode = SelectableSelectionMode.Both;

    public bool CanSelectFromClick => selectionMode != SelectableSelectionMode.DragOnly;
    public bool CanSelectFromDrag => selectionMode != SelectableSelectionMode.ClickOnly;

    public virtual void OnSelected()
    {
        
    }
    
    public virtual void OnDeselected()
    {
        
    }
    
    public GameObject GetGameObject()
    {
        return gameObject;
    }
    
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}

public enum SelectableSelectionMode
{
    Both,
    ClickOnly,
    DragOnly
}

