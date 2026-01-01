public partial class UnitHarvesterController
{
    private sealed class UnitHarvesterHarvestingState : IUnitHarvesterControllerState
    {
        void IUnitHarvesterControllerState.Enter(UnitHarvesterController controller)
        {
            controller.movement?.StopMovement();
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

            if (!controller.IsInRange(controller.currentTarget))
            {
                controller.SetState(movingToTargetState);
                return;
            }

            controller.movement.StopMovement();
            if (!controller.CanHarvest())
                return;

            int harvested = controller.Harvest(controller.currentTarget);
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
            }
        }

        void IUnitHarvesterControllerState.Exit(UnitHarvesterController controller)
        {
        }
    }
}
