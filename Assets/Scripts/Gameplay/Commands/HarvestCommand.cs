using UnityEngine;

public class HarvestCommand : BaseCommand
{
    private IHarvestable target;
    
    public HarvestCommand(IHarvestable target)
    {
        this.target = target;
    }
    
    public override void Execute(CommandExecutor executor)
    {
        if (executor == null)
        {
            Debug.LogError("HarvestCommand: Executor is null");
            return;
        }
        
        if (target == null || target.GetGameObject() == null)
        {
            Debug.LogError("HarvestCommand: Target is null or destroyed");
            Complete();
            return;
        }
        
        if (target == null || target.GetGameObject() == null || target.IsDepleted())
        {
            Complete();
            return;
        }
        
        if (executor.TryGetCapability(out IHarvestCapability harvestUnit))
        {
            harvestUnit.StartHarvest(target);
        }
        else
        {
            Debug.LogWarning($"HarvestCommand: IHarvestUnit capability not found");
            Complete();
        }
    }
    
    protected override void OnUpdate(CommandExecutor executor)
    {
        if (executor == null)
        {
            Complete();
            return;
        }
        
        if (!executor.TryGetCapability(out IHarvestCapability harvestUnit))
        {
            Complete();
            return;
        }
        
        harvestUnit.TickHarvest();
        if (!harvestUnit.IsHarvesting)
        {
            Complete();
        }
    }
    
    public override string GetDescription()
    {
        if (target == null || target.GetGameObject() == null)
            return "Harvest (target destroyed)";
            
        return $"Harvest {target.GetGameObject().name}";
    }

    protected override void OnCancel(CommandExecutor executor)
    {
        base.OnCancel(executor);
        if (executor != null && executor.TryGetCapability(out IHarvestCapability harvestUnit))
        {
            harvestUnit.StopHarvest();
        }
    }
}

