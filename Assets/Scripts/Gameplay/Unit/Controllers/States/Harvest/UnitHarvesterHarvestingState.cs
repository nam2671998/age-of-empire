public partial class UnitHarvesterController
{
    private sealed class UnitHarvesterHarvestingState : IUnitHarvesterControllerState
    {
        string IUnitHarvesterControllerState.Name  => "Harvest";
        void IUnitHarvesterControllerState.Enter(UnitHarvesterController controller)
        {
            controller.movementOwner?.StopMovement();
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

            if (controller.movementOwner == null || controller.movementOwner.GetTransform() == null)
            {
                controller.StopHarvest();
                return;
            }

            if (!controller.IsInRange(controller.currentHarvestPosition))
            {
                controller.SetState(movingToTargetState);
                return;
            }

            controller.FaceTarget(controller.currentTarget);
            controller.movementOwner.StopMovement();
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
