using UnityEngine;

public partial class UnitHarvesterController
{
    private sealed class UnitHarvesterMovingToDepositState : IUnitHarvesterControllerState
    {
        void IUnitHarvesterControllerState.Enter(UnitHarvesterController controller)
        {
            controller.currentHarvestPosition = Vector3.positiveInfinity;
        }

        void IUnitHarvesterControllerState.Tick(UnitHarvesterController controller)
        {
            if (controller.inventoryCount <= 0)
            {
                controller.depositTarget = null;
                if (controller.HasValidTarget && !controller.IsTargetDepleted)
                {
                    controller.SetState(movingToTargetState);
                    return;
                }

                controller.SetState(searchingState);
                return;
            }

            if (controller.movementOwner == null || controller.movementOwner.GetTransform() == null)
            {
                controller.StopHarvest();
                return;
            }

            if (!controller.HasValidDepositTarget)
            {
                if (!controller.TryStartDepositing())
                {
                    controller.SetState(searchingState);
                    return;
                }
            }

            if (controller.IsInDepositRange(controller.depositTarget))
            {
                controller.movementOwner.StopMovement();
                controller.SetState(depositingState);
                return;
            }

            controller.movementOwner.MoveTo(controller.depositTarget.GetNearestDepositPosition(controller.movementOwner.GetTransform().position), controller.depositTarget.GetDepositRadius());
        }

        void IUnitHarvesterControllerState.Exit(UnitHarvesterController controller)
        {
        }
    }
}
