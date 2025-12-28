using UnityEngine;

public class MoveCommand : BaseCommand
{
    private Vector3 targetPosition;
    private float stoppingDistance = 0;
    private Vector2Int reservedGridCell;
    private bool hasReservedCell = false;
    
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

        // Free any previous cell reservation for this unit
        if (GridManager.Instance != null)
        {
            GridManager.Instance.FreeUnitReservation(executor);
        }

        // Find nearest free cell and reserve it
        if (GridManager.Instance != null)
        {
            reservedGridCell = GridManager.Instance.FindNearestFreeCell(targetPosition, executor);
            GridManager.Instance.ReserveCell(reservedGridCell, executor);
            hasReservedCell = true;

            // Update target position to the center of the reserved cell
            targetPosition = GridManager.Instance.GridToWorld(reservedGridCell);
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
        
        // Free the reserved cell when command is cancelled
        FreeReservedCell(executor);
        
        if (executor != null && executor.TryGetCapability(out IMovementCapability movement))
        {
            movement.StopMovement();
        }
    }

    private void FreeReservedCell(CommandExecutor executor)
    {
        if (hasReservedCell && GridManager.Instance != null && executor != null)
        {
            GridManager.Instance.FreeUnitReservation(executor);
            hasReservedCell = false;
        }
    }
}