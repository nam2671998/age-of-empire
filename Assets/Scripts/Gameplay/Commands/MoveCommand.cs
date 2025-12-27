using UnityEngine;

public class MoveCommand : BaseCommand
{
    private Vector3 targetPosition;
    private float stoppingDistance = 0;
    
    public MoveCommand(Vector3 position, float stoppingDistance = 0.5f)
    {
        this.targetPosition = position;
        this.stoppingDistance = stoppingDistance;
    }
    
    public override void Execute(CommandExecutor executor)
    {
        if (executor == null || !executor.TryGetCapability(out IMovementCapability movement))
        {
            Debug.LogError("MoveCommand: IMovementCapability not found");
            return;
        }
        
        movement.MoveTo(targetPosition, stoppingDistance);
    }
    
    protected override void OnUpdate(CommandExecutor executor)
    {
        if (executor == null || !executor.TryGetCapability(out IMovementCapability movement))
        {
            Complete();
            return;
        }

        if (!movement.IsMoving)
        {
            Complete();
        }
        
        float distanceToTarget = Vector3.Distance(movement.transform.position, targetPosition);
        if (distanceToTarget <= stoppingDistance)
        {
            Complete();
        }
    }
    
    public override string GetDescription()
    {
        return $"Move to {targetPosition}";
    }

    public override void Cancel(CommandExecutor executor)
    {
        base.Cancel(executor);
        if (executor != null && executor.TryGetCapability(out IMovementCapability movement))
        {
            movement.StopMovement();
        }
    }
}

