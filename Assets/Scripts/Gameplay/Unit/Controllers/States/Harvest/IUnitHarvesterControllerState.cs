public partial class UnitHarvesterController
{
    private interface IUnitHarvesterControllerState
    {
        void Enter(UnitHarvesterController controller);
        void Tick(UnitHarvesterController controller);
        void Exit(UnitHarvesterController controller);
    }
}

