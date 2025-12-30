using UnityEngine;

public class BuildCommand : BaseCommand
{
    private IBuildable target;
    private float buildCooldown = 1f;
    private float lastBuildTime = 0f;
    private CommandExecutor reservedExecutor;
    private Vector3 reservedBuildPosition;
    private bool hasReservedBuildPosition;
    private float buildStoppingDistance = 0.25f;
    private float lastReserveAttemptTime = float.NegativeInfinity;

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

            reservedExecutor = executor;
            TryReservePosition(executor);

            if (executor.TryGetCapability(out IMovementCapability movement))
            {
                movement.MoveTo(reservedBuildPosition, buildStoppingDistance);
            }
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

        if (!hasReservedBuildPosition && Time.time >= lastReserveAttemptTime + 0.5f)
        {
            TryReservePosition(executor);
        }

        Vector3 unitPosition = movement.transform.position;
        unitPosition.y = reservedBuildPosition.y;
        float distance = Vector3.Distance(unitPosition, reservedBuildPosition);

        if (distance <= buildStoppingDistance)
        {
            movement.StopMovement();
            if (Time.time >= lastBuildTime + buildCooldown && buildUnit.CanBuild())
            {
                buildUnit.Build(target);
                lastBuildTime = Time.time;
            }
            return;
        }

        if (!movement.IsMoving)
        {
            movement.MoveTo(reservedBuildPosition, buildStoppingDistance);
        }
    }
    
    protected override void OnCancel(CommandExecutor executor)
    {
        base.OnCancel(executor);
        if (executor != null && executor.TryGetCapability(out IBuildCapability buildUnit))
        {
            buildUnit.StopBuilding();
        }

        ReleaseReservation();
    }

    protected override void Complete()
    {
        ReleaseReservation();
        base.Complete();
    }
    
    public override string GetDescription()
    {
        if (target == null || target.GetGameObject() == null)
        {
            return "Build (target missing)";
        }

        return $"Build {target.GetGameObject().name}";
    }

    private void TryReservePosition(CommandExecutor executor)
    {
        lastReserveAttemptTime = Time.time;

        if (target != null && target.TryReserveBuildPosition(executor, out Vector3 position))
        {
            reservedBuildPosition = position;
            hasReservedBuildPosition = true;
            return;
        }

        if (hasReservedBuildPosition && target != null)
        {
            target.ReleaseBuildPosition(executor);
        }

        hasReservedBuildPosition = false;
        reservedBuildPosition = target != null ? target.GetNearestBuildPosition(executor.transform.position) : executor.transform.position;
    }

    private void ReleaseReservation()
    {
        if (!hasReservedBuildPosition)
        {
            return;
        }

        if (target != null && !ReferenceEquals(reservedExecutor, null))
        {
            target.ReleaseBuildPosition(reservedExecutor);
        }

        hasReservedBuildPosition = false;
        reservedExecutor = null;
    }
}

