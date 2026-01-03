using UnityEngine;

public partial class UnitHarvesterController
{
    private class UnitHarvesterMovingToDepositState : IUnitHarvesterControllerState
    {
        string IUnitHarvesterControllerState.Name  => "Move To Deposit";
        void IUnitHarvesterControllerState.Enter(UnitHarvesterController controller)
        {
            
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

            controller.movementOwner.MoveTo(controller.depositTarget.GetNearestDepositPosition(controller.movementOwner.GetTransform().position), 0.1f);
        }

        void IUnitHarvesterControllerState.Exit(UnitHarvesterController controller)
        {
        }
    }
}
