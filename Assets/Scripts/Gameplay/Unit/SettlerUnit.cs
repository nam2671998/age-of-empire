using UnityEngine;

[RequireComponent(typeof(SelectableObject))]
[RequireComponent(typeof(UnitMovementController))]
[RequireComponent(typeof(UnitActionStateController))]
[RequireComponent(typeof(UnitCombatController))]
[RequireComponent(typeof(UnitHarvesterController))]
[RequireComponent(typeof(UnitBuilderController))]
public class SettlerUnit : Unit, IStopAction
{
    private UnitCombatController combat;
    private UnitHarvesterController harvester;
    private UnitBuilderController builder;
    
    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        TryGetComponent(out combat);
        TryGetComponent(out harvester);
        TryGetComponent(out builder);
    }
    
    protected override void InitializeUnit()
    {
        if (combat != null)
        {
            CloseRangeStrategy closeRangeStrategy = new CloseRangeStrategy();
            combat.SetAttackStrategy(closeRangeStrategy);
            combat.SetDistanceStrategy(closeRangeStrategy);
        }
    }
    
    protected override void UpdateUnit()
    {
    }
    
    public void StopOtherActions()
    {
        if (combat != null)
        {
            combat.StopAttacking();
        }
        
        if (harvester != null)
        {
            harvester.StopHarvesting();
        }
        
        if (builder != null)
        {
            builder.StopBuilding();
        }
    }
}
