using UnityEngine;

/// <summary>
/// Interface for GameObjects that can be selected via box selection
/// </summary>
public interface IGameSelectable
{
    /// <summary>
    /// Called when the object is selected
    /// </summary>
    void OnSelected();
    
    /// <summary>
    /// Called when the object is deselected
    /// </summary>
    void OnDeselected();
    
    /// <summary>
    /// Gets the GameObject associated with this selectable
    /// </summary>
    GameObject GetGameObject();
    
    /// <summary>
    /// Gets the world position of the selectable (used for selection bounds checking)
    /// </summary>
    Vector3 GetPosition();

    bool CanSelectFromDrag { get; }
    bool CanSelectFromClick { get; }
}

