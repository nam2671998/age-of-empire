public partial class UnitHarvesterController
{
    private sealed class UnitHarvesterMovingToTargetState : IUnitHarvesterControllerState
    {
        void IUnitHarvesterControllerState.Enter(UnitHarvesterController controller)
        {
            
        }

        void IUnitHarvesterControllerState.Tick(UnitHarvesterController controller)
        {
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

            if (controller.IsInRange(controller.currentTarget))
            {
                controller.movement.StopMovement();
                controller.SetState(harvestingState);
                return;
            }

            controller.movement.MoveTo(controller.currentTarget.GetHarvestPosition(), controller.currentTarget.GetHarvestRadius());
        }

        void IUnitHarvesterControllerState.Exit(UnitHarvesterController controller)
        {
        }
    }
}
