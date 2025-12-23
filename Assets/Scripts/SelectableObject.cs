using UnityEngine;

/// <summary>
/// Component that implements IGameSelectable for easy attachment to GameObjects
/// </summary>
public class SelectableObject : MonoBehaviour, IGameSelectable
{
    private bool isSelected = false;
    
    public void OnSelected()
    {
        isSelected = true;
        // Override this method in derived classes or use events for custom behavior
    }
    
    public void OnDeselected()
    {
        isSelected = false;
        // Override this method in derived classes or use events for custom behavior
    }
    
    public GameObject GetGameObject()
    {
        return gameObject;
    }
    
    public Vector3 GetPosition()
    {
        return transform.position;
    }
    
    public Bounds GetBounds()
    {
        // Return a minimal bounds centered on the transform position
        // The selection system will use transform position directly
        return new Bounds(transform.position, Vector3.zero);
    }
    
    public bool IsSelected()
    {
        return isSelected;
    }
}

