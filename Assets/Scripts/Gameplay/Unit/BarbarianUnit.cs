using UnityEngine;

[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(UnitCombatController))]
[RequireComponent(typeof(CommandExecutor))]
public class BarbarianUnit : Unit, IStopAction
{
    [SerializeField] private float attackWindUpDuration = 0;
    private ICombatCapability combat;

    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        TryGetComponent(out combat);
        if (TryGetComponent(out Damageable damageable))
        {
            damageable.OnDamageTakenHandler += Revenge;
        }
    }

    private void Revenge()
    {
        if (TryGetComponent(out CommandExecutor commandExecutor))
        {
            commandExecutor.SetCommand(new AttackCommand(null));
        }
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
