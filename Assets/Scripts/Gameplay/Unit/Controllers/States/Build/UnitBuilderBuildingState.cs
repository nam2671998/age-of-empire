using UnityEngine;

public partial class UnitBuilderController
{
    private class UnitBuilderBuildingState : IUnitBuilderControllerState
    {
        public void Enter(UnitBuilderController controller)
        {
            controller.animator?.TriggerBuild();
        }

        public void Tick(UnitBuilderController controller)
        {
            if (!controller.HasValidTarget || controller.currentTarget.IsComplete())
            {
                controller.StopBuilding();
                return;
            }

            if (controller.movementOwner != null && controller.movementOwner.IsMoving)
            {
                controller.movementOwner.StopMovement();
            }

            if (!controller.IsInRange(controller.currentTarget))
            {
                controller.SetState(movingToTargetState);
                return;
            }

            if (!controller.CanBuild())
            {
                return;
            }

            controller.FaceTarget(controller.currentTarget);
            bool progressed = controller.currentTarget.Build(controller.buildingPower);
            controller.lastBuildTime = Time.time;

            if (controller.animator != null)
            {
                if (progressed && !controller.currentTarget.IsComplete())
                {
                    controller.animator.TriggerBuild();
                }
                else
                {
                    controller.animator.TriggerIdle();
                }
            }

            if (controller.currentTarget.IsComplete())
            {
                controller.StopBuilding();
            }
        }

        public void Exit(UnitBuilderController controller)
        {
        }
    }
}