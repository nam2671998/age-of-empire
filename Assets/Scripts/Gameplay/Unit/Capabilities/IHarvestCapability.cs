public interface IHarvestCapability
{
    void SetHarvestTarget(IHarvestable target);
    /// <summary>
    /// Harvest a resource
    /// </summary>
    /// <param name="target">Resource to harvest</param>
    /// <returns>The amount of resource harvested in one hit</returns>
    int Harvest(IHarvestable target);
    bool IsInRange(IHarvestable target);
    bool CanHarvest();
    void StopHarvest();
}

