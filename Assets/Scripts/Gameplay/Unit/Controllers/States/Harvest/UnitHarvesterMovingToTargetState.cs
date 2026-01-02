using UnityEngine;

public partial class UnitHarvesterController
{
    private sealed class UnitHarvesterMovingToTargetState : IUnitHarvesterControllerState
    {
        void IUnitHarvesterControllerState.Enter(UnitHarvesterController controller)
        {
            controller.currentHarvestPosition = Vector3.positiveInfinity;
        }

        void IUnitHarvesterControllerState.Tick(UnitHarvesterController controller)
        {
            if (controller.IsInventoryFull)
            {
                if (!controller.TryStartDepositing())
                {
                    controller.SetState(searchingState);
                }
                return;
            }

            if (!controller.HasValidTarget || controller.IsTargetDepleted)
            {
                controller.SetState(searchingState);
                return;
            }

            if (controller.movementOwner == null || controller.movementOwner.GetTransform() == null)
            {
                controller.StopHarvest();
                return;
            }

            if (controller.IsInRange(controller.currentHarvestPosition))
            {
                controller.movementOwner.StopMovement();
                controller.SetState(harvestingState);
                return;
            }

            if (controller.currentTarget.TryGetHarvestPosition(out Vector3 harvestPosition))
            {
                controller.currentHarvestPosition = harvestPosition;
                controller.movementOwner.MoveTo(harvestPosition, 0.1f);
                GridManager.Instance.ReserveCell(GridManager.Instance.WorldToGrid(harvestPosition), controller.movementOwner);
            }
        }

        void IUnitHarvesterControllerState.Exit(UnitHarvesterController controller)
        {
            GridManager.Instance.FreeUnitReservation(controller.movementOwner);
        }
    }
}
