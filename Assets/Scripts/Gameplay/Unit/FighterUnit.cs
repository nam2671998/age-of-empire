using UnityEngine;

[RequireComponent(typeof(UnitMovementController))]
[RequireComponent(typeof(UnitActionStateController))]
[RequireComponent(typeof(UnitCombatController))]
[RequireComponent(typeof(Damageable))]
public class FighterUnit : Unit, IStopAction
{
    private UnitCombatController combat;
    
    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        TryGetComponent(out combat);
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
    }
}
