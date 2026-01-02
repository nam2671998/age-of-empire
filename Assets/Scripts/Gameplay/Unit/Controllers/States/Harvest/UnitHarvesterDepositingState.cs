public partial class UnitHarvesterController
{
    private sealed class UnitHarvesterDepositingState : IUnitHarvesterControllerState
    {
        void IUnitHarvesterControllerState.Enter(UnitHarvesterController controller)
        {
            controller.movementOwner?.StopMovement();
        }

        void IUnitHarvesterControllerState.Tick(UnitHarvesterController controller)
        {
            controller.movementOwner?.StopMovement();

            if (!controller.HasValidDepositTarget)
            {
                controller.StopHarvest();
                return;
            }

            if (!controller.IsInDepositRange(controller.depositTarget))
            {
                controller.SetState(movingToDepositState);
                return;
            }

            controller.DepositAll();
            controller.depositTarget = null;

            if (controller.HasValidTarget && !controller.IsTargetDepleted)
            {
                controller.SetState(movingToTargetState);
                return;
            }

            controller.SetState(searchingState);
        }

        void IUnitHarvesterControllerState.Exit(UnitHarvesterController controller)
        {
        }
    }
}

