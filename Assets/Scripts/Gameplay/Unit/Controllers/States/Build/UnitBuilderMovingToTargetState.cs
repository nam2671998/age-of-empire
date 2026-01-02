using UnityEngine;

public partial class UnitBuilderController
{
    private class UnitBuilderMovingToTargetState : IUnitBuilderControllerState
    {
        public void Enter(UnitBuilderController controller)
        {
            controller.animator?.TriggerIdle();
            controller.TryReservePosition();
        }

        public void Tick(UnitBuilderController controller)
        {
            if (!controller.HasValidTarget || controller.currentTarget.IsComplete())
            {
                controller.StopBuilding();
                return;
            }

            if (controller.movementOwner == null)
            {
                controller.StopBuilding();
                return;
            }

            if (!controller.hasReservedBuildPosition && Time.time >= controller.lastReserveAttemptTime + 0.5f)
            {
                controller.TryReservePosition();
            }

            if (controller.IsInRange(controller.currentTarget))
            {
                controller.movementOwner.StopMovement();
                controller.SetState(buildingState);
                return;
            }

            if (!controller.movementOwner.IsMoving)
            {
                controller.movementOwner.MoveTo(controller.reservedBuildPosition, controller.buildStoppingDistance);
            }
        }

        public void Exit(UnitBuilderController controller)
        {
        }
    }
}