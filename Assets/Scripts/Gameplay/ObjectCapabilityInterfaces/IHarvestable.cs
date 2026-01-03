using UnityEngine;

/// <summary>
/// Interface for objects that can be harvested/collected
/// </summary>
public interface IHarvestable
{
    /// <summary>
    /// Harvests resources from this object
    /// </summary>
    /// <param name="amount">The amount to harvest</param>
    /// <returns>The amount actually harvested</returns>
    int Harvest(int amount);
    
    /// <summary>
    /// Gets the resource type
    /// </summary>
    ResourceType GetResourceType();
    
    /// <summary>
    /// Checks if the resource is depleted
    /// </summary>
    bool IsDepleted();
    
    /// <summary>
    /// Gets the GameObject associated with this harvestable
    /// </summary>
    GameObject GetGameObject();
    
    /// <summary>
    /// Gets the world position of the harvestable
    /// </summary>
    Vector3 GetPosition();

    /// <summary>
    /// Gets the position where a unit should stand when harvesting this resource
    /// </summary>
    /// <param name="harvestPosition"></param>
    /// <param name="harvester"></param>
    bool TryGetHarvestPosition(out Vector3 harvestPosition, IGridEntity harvester);

    /// <summary>
    /// Checks if the resource can be harvested
    /// </summary>
    /// <returns></returns>
    bool CanHarvest();
}

/// <summary>
/// Types of resources that can be harvested
/// </summary>
public enum ResourceType
{
    None,
    Wood = 1,
    Stone = 2,
    Food = 3
}

