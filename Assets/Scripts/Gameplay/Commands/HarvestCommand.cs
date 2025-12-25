using UnityEngine;

public class HarvestCommand : BaseCommand
{
    private IHarvestable target;
    private float harvestRange = 2f;
    private float harvestCooldown = 1f;
    private float lastHarvestTime = float.NegativeInfinity;
    private float moveToRange = 3f;
    
    public HarvestCommand(IHarvestable target, float harvestRange = 2f, float harvestCooldown = 1f)
    {
        this.target = target;
        this.harvestRange = harvestRange;
        this.harvestCooldown = 1f;
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
            Debug.LogWarning("HarvestCommand: Target is null or destroyed");
            Complete();
            return;
        }
        
        if (executor.TryGetCapability(out IHarvestCapability harvestUnit))
        {
            harvestUnit.SetHarvestTarget(target);
            lastHarvestTime = Time.time - harvestCooldown;
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
        
        if (target == null || target.GetGameObject() == null || target.IsDepleted())
        {
            Complete();
            return;
        }
        
        if (!executor.TryGetCapability(out IHarvestCapability harvestUnit))
        {
            Complete();
            return;
        }
        
        if (!executor.TryGetCapability(out IMovementCapability movement))
        {
            Complete();
            return;
        }
        
        Vector3 harvestPosition = target.GetHarvestPosition();
        
        if (harvestUnit.IsInRange(target))
        {
            if (Time.time >= lastHarvestTime + harvestCooldown && harvestUnit.CanHarvest())
            {
                harvestUnit.Harvest(target);
                lastHarvestTime = Time.time;
            }
        }
        else
        {
            movement.MoveTo(harvestPosition, target.GetHarvestRadius());
        }
    }
    
    public override string GetDescription()
    {
        if (target == null || target.GetGameObject() == null)
            return "Harvest (target destroyed)";
            
        return $"Harvest {target.GetGameObject().name}";
    }
    
    public IHarvestable GetTarget()
    {
        return target;
    }
}

