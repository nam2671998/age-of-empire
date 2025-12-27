using UnityEngine;

/// <summary>
/// Component that implements IGameSelectable for easy attachment to GameObjects
/// </summary>
public class SelectableObject : MonoBehaviour, IGameSelectable
{
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

