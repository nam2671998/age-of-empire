using UnityEngine;

[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(UnitCombatController))]
public class BarbarianUnit : Unit, IStopAction
{
    [SerializeField] private float attackWindUpDuration = 0;
    private ICombatCapability combat;

    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        TryGetComponent(out combat);
    }

    protected override void InitializeUnit()
    {
        if (combat != null)
        {
            CloseRangeStrategy strategy = new CloseRangeStrategy(attackWindUpDuration);
            combat.SetAttackStrategy(strategy);
            combat.SetDistanceStrategy(strategy);
        }
    }

    public void StopOtherActions()
    {
        if (combat != null)
        {
            combat.StopAttacking();
        }
    }
}
