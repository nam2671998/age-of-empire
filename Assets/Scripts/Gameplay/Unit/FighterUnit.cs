using UnityEngine;

[RequireComponent(typeof(SelectableObject))]
[RequireComponent(typeof(UnitMovementController))]
[RequireComponent(typeof(UnitActionStateController))]
[RequireComponent(typeof(UnitCombatController))]
public class FighterUnit : Unit
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
}
