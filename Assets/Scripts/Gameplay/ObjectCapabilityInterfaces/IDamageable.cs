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
    /// <param name="attacker">The unit that attacked</param>
    void TakeDamage(float damage, Unit attacker);
    
    /// <summary>
    /// Gets the current health of the object
    /// </summary>
    float GetHealth();
    
    /// <summary>
    /// Gets the maximum health of the object
    /// </summary>
    float GetMaxHealth();
    
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

