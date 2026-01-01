public partial class UnitHarvesterController
{
    private sealed class UnitHarvesterDepositingState : IUnitHarvesterControllerState
    {
        void IUnitHarvesterControllerState.Enter(UnitHarvesterController controller)
        {
            controller.movement?.StopMovement();
        }

        void IUnitHarvesterControllerState.Tick(UnitHarvesterController controller)
        {
            controller.movement?.StopMovement();

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

            if (controller.currentTarget != null)
            {
                controller.SetState(searchingState);
                return;
            }

            controller.StopHarvest();
        }

        void IUnitHarvesterControllerState.Exit(UnitHarvesterController controller)
        {
        }
    }
}

