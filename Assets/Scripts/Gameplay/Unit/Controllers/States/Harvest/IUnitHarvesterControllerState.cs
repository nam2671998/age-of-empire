public partial class UnitHarvesterController
{
    private interface IUnitHarvesterControllerState
    {
        string Name { get; }
        void Enter(UnitHarvesterController controller);
        void Tick(UnitHarvesterController controller);
        void Exit(UnitHarvesterController controller);
    }
}

