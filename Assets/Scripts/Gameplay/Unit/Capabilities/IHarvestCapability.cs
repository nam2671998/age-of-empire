public interface IHarvestCapability
{
    bool IsHarvesting { get; }
    void StartHarvest(IHarvestable target);
    void TickHarvest();
    void StopHarvest();
}

