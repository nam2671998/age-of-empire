public partial class UnitBuilderController
{
    private class UnitBuilderIdleState : IUnitBuilderControllerState
    {
        public void Enter(UnitBuilderController controller)
        {
            controller.animator?.TriggerIdle();
        }

        public void Tick(UnitBuilderController controller)
        {
        }

        public void Exit(UnitBuilderController controller)
        {
        }
    }
}
