using UnityEngine;

public class BuildCommand : BaseCommand
{
    private readonly IBuildable target;

    public BuildCommand(IBuildable target)
    {
        this.target = target;
    }
    
    public override void Execute(CommandExecutor executor)
    {
        if (executor == null)
        {
            return;
        }

        if (target == null || target.GetGameObject() == null || target.IsComplete())
        {
            Complete();
            return;
        }
        
        if (executor.TryGetCapability(out IBuildCapability buildUnit))
        {
            buildUnit.SetBuildTarget(target);
        }
        else
        {
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
            if (executor.TryGetCapability(out IBuildCapability missingTargetBuildUnit))
            {
                missingTargetBuildUnit.StopBuilding();
            }
            Complete();
            return;
        }
        
        if (!executor.TryGetCapability(out IBuildCapability buildUnit))
        {
            Complete();
            return;
        }

        buildUnit.Build(target);

        if (target.IsComplete())
        {
            buildUnit.StopBuilding();
            Complete();
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
        if (target == null || target.GetGameObject() == null)
        {
            return "Build (target missing)";
        }

        return $"Build {target.GetGameObject().name}";
    }
}
