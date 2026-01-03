using UnityEngine;

[RequireComponent(typeof(UnitMovementController))]
[RequireComponent(typeof(UnitActionStateController))]
[RequireComponent(typeof(UnitCombatController))]
[RequireComponent(typeof(UnitHarvesterController))]
[RequireComponent(typeof(UnitBuilderController))]
[RequireComponent(typeof(Damageable))]
public class SettlerUnit : Unit, IStopAction
{
    [SerializeField] private float attackWindUpDuration = 0;
    private ICombatCapability combat;
    private IHarvestCapability harvester;
    private IBuildCapability builder;
    
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
            CloseRangeStrategy closeRangeStrategy = new CloseRangeStrategy(attackWindUpDuration);
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
            harvester.StopHarvest();
        }
        
        if (builder != null)
        {
            builder.StopBuilding();
        }
    }
}
