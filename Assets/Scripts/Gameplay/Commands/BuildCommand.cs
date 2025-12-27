using UnityEngine;

public class BuildCommand : BaseCommand
{
    private IBuildable target;
    private float buildCooldown = 1f;
    private float lastBuildTime = 0f;

    public BuildCommand(IBuildable target, float buildCooldown = 1f)
    {
        this.target = target;
        this.buildCooldown = buildCooldown;
    }
    
    public override void Execute(CommandExecutor executor)
    {
        if (executor == null)
        {
            Debug.LogError("BuildCommand: Executor is null");
            return;
        }

        if (target == null)
        {
            Debug.LogError("BuildCommand: Target is null");
            Complete();
            return;
        }
        
        if (executor.TryGetCapability(out IBuildCapability buildUnit))
        {
            buildUnit.SetBuildTarget(target);
            lastBuildTime = Time.time - buildCooldown;
        }
        else
        {
            Debug.LogWarning($"BuildCommand: IBuildUnit capability not found");
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
        
        if (target == null || target.GetGameObject() == null || target.IsComplete())
        {
            Complete();
            return;
        }
        
        if (!executor.TryGetCapability(out IBuildCapability buildUnit))
        {
            Complete();
            return;
        }
        
        if (!executor.TryGetCapability(out IMovementCapability movement))
        {
            Complete();
            return;
        }
        
        Vector3 buildPosition = target.GetNearestBuildPosition(buildUnit.GetGameObject().transform.position);
        if (buildUnit.IsInRange(target))
        {
            movement.StopMovement();
            if (Time.time >= lastBuildTime + buildCooldown && buildUnit.CanBuild())
            {
                buildUnit.Build(target);
                lastBuildTime = Time.time;
            }
        }
        else
        {
            movement.MoveTo(buildPosition, 0);
        }
    }
    
    protected override void OnCancel(CommandExecutor executor)
    {
        base.OnCancel(executor);
        if (executor != null && executor.TryGetCapability(out IBuildCapability buildUnit))
        {
            buildUnit.StopBuilding();
        }
    }
    
    public override string GetDescription()
    {
        return $"Build {target.GetGameObject().name}";
    }
}

