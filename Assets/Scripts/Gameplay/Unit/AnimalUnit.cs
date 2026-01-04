using UnityEngine;

[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(UnitCombatController))]
[RequireComponent(typeof(CommandExecutor))]
public class AnimalUnit : Unit, IStopAction
{
    [SerializeField] private GameObject meatPrefab;
    [SerializeField] private float attackWindUpDuration = 0;
    private ICombatCapability combat;

    protected override void InitializeComponents()
    {
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

        if (TryGetComponent(out Damageable damageable))
        {
            damageable.OnDeath += OnDeath;
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

    private void OnDeath()
    {
        ObjectPool.Spawn(meatPrefab, transform.position, Quaternion.identity);
    }

    public void StopOtherActions()
    {
        if (combat != null)
        {
            combat.StopAttacking();
        }
    }
}
