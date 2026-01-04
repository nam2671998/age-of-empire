using UnityEngine;

/// <summary>
/// Interface for objects that can be attacked
/// </summary>
public interface IDamageable : IFactionOwner
{
    /// <summary>
    /// Called when the object is attacked
    /// </summary>
    /// <param name="damage">The amount of damage dealt</param>
    void TakeDamage(int damage);
    
    Collider HitCollider { get; }
    
    /// <summary>
    /// Checks if the object is destroyed
    /// </summary>
    bool IsDestroyed();
    
    /// <summary>
    /// Gets the GameObject associated with this attackable
    /// </summary>
    GameObject GetGameObject();
    
    /// <summary>
    /// Gets the world position of the attackable
    /// </summary>
    Vector3 GetPosition();
}

