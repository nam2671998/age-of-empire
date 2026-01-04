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
        if (executor == null || !executor.TryGetCapability(out IMovementCapability movementOwner))
        {
            return;
        }

        // Free any previous cell reservation for this unit
        if (GridManager.Instance != null)
        {
            GridManager.Instance.FreeUnitReservation(movementOwner);
        }

        // Find nearest free cell and reserve it
        if (GridManager.Instance != null)
        {
            reservedGridCell = GridManager.Instance.FindNearestFreeCell(targetPosition, movementOwner);
            GridManager.Instance.ReserveCell(reservedGridCell, movementOwner);
            hasReservedCell = true;

            // Update target position to the center of the reserved cell
            targetPosition = GridManager.Instance.GridToWorld(reservedGridCell);
        }
        
        movementOwner.MoveTo(targetPosition, stoppingDistance);
    }
    
    protected override void OnUpdate(CommandExecutor executor)
    {
        if (executor == null || !executor.TryGetCapability(out IMovementCapability movementOwner))
        {
            Complete();
            return;
        }

        if (!movementOwner.IsMoving)
        {
            Complete();
            return;
        }
        
        Transform movementOwnerTransform = movementOwner.GetTransform();
        if (movementOwnerTransform == null)
        {
            Complete();
            return;
        }
            
        float distanceToTarget = Vector3.Distance(movementOwnerTransform.position, targetPosition);
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
        
        if (executor != null && executor.TryGetCapability(out IMovementCapability movementOwner))
        {
            FreeReservedCell(movementOwner);
            movementOwner.StopMovement();
        }
    }

    private void FreeReservedCell(IMovementCapability movementOwner)
    {
        if (hasReservedCell && GridManager.Instance != null && movementOwner != null)
        {
            GridManager.Instance.FreeUnitReservation(movementOwner);
            hasReservedCell = false;
        }
    }
}