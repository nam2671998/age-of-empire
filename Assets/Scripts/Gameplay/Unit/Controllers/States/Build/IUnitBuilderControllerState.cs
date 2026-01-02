public partial class UnitBuilderController
{
    private interface IUnitBuilderControllerState
    {
        void Enter(UnitBuilderController controller);
        void Tick(UnitBuilderController controller);
        void Exit(UnitBuilderController controller);
    }
}