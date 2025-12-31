public partial class UnitHarvesterController
{
    private sealed class UnitHarvesterIdleState : IUnitHarvesterControllerState
    {
        void IUnitHarvesterControllerState.Enter(UnitHarvesterController controller)
        {
            controller.Idle();
        }

        void IUnitHarvesterControllerState.Tick(UnitHarvesterController controller)
        {
        }

        void IUnitHarvesterControllerState.Exit(UnitHarvesterController controller)
        {
        }
    }
}
