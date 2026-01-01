using UnityEngine;

public partial class UnitHarvesterController
{
    private sealed class UnitHarvesterMovingToTargetState : IUnitHarvesterControllerState
    {
        void IUnitHarvesterControllerState.Enter(UnitHarvesterController controller)
        {
            
        }

        void IUnitHarvesterControllerState.Tick(UnitHarvesterController controller)
        {
            if (controller.IsInventoryFull)
            {
                if (!controller.TryStartDepositing())
                {
                    controller.StopHarvest();
                }
                return;
            }

            if (!controller.HasValidTarget || controller.IsTargetDepleted)
            {
                controller.SetState(searchingState);
                return;
            }

            if (controller.movement == null)
            {
                controller.StopHarvest();
                return;
            }

            if (controller.IsInRange(controller.currentHarvestPosition))
            {
                controller.movement.StopMovement();
                controller.SetState(harvestingState);
                return;
            }

            if (controller.currentTarget.TryGetHarvestPosition(out Vector3 harvestPosition))
            {
                controller.currentHarvestPosition = harvestPosition;
                controller.movement.MoveTo(harvestPosition, 0.1f);
                GridManager.Instance.ReserveCell(GridManager.Instance.WorldToGrid(harvestPosition), controller.movement);
            }
        }

        void IUnitHarvesterControllerState.Exit(UnitHarvesterController controller)
        {
            GridManager.Instance.FreeUnitReservation(controller.movement);
        }
    }
}
