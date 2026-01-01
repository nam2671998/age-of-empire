public partial class UnitHarvesterController
{
    private sealed class UnitHarvesterMovingToDepositState : IUnitHarvesterControllerState
    {
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

                if (controller.currentTarget != null)
                {
                    controller.SetState(searchingState);
                    return;
                }

                controller.StopHarvest();
                return;
            }

            if (controller.movement == null)
            {
                controller.StopHarvest();
                return;
            }

            if (!controller.HasValidDepositTarget)
            {
                if (!controller.TryStartDepositing())
                {
                    controller.StopHarvest();
                    return;
                }
            }

            if (controller.IsInDepositRange(controller.depositTarget))
            {
                controller.movement.StopMovement();
                controller.SetState(depositingState);
                return;
            }

            controller.movement.MoveTo(controller.depositTarget.GetDepositPosition(), controller.depositTarget.GetDepositRadius());
        }

        void IUnitHarvesterControllerState.Exit(UnitHarvesterController controller)
        {
        }
    }
}
