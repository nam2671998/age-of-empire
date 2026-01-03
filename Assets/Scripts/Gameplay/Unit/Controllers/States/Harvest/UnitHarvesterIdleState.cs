public partial class UnitHarvesterController
{
    private class UnitHarvesterIdleState : IUnitHarvesterControllerState
    {
        string IUnitHarvesterControllerState.Name  => "Idle";
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
