using UnityEngine;

[RequireComponent(typeof(UnitCombatController))]
[RequireComponent(typeof(Damageable))]
public class AnimalUnit : Unit, IStopAction
{
    private ICombatCapability combat;
    private Damageable damageable;

    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        TryGetComponent(out combat);
        TryGetComponent(out damageable);
    }

    protected override void InitializeUnit()
    {
        if (combat != null)
        {
            CloseRangeStrategy strategy = new CloseRangeStrategy();
            combat.SetAttackStrategy(strategy);
            combat.SetDistanceStrategy(strategy);
        }

        if (damageable != null)
        {
            damageable.OnDamageTakenHandler += OnDamageTaken;
        }
    }

    private void OnDamageTaken(Unit attacker)
    {
        if (attacker != null && combat != null)
        {
            // Retaliate if not already attacking someone else? Or switch target?
            // Switch target to latest aggressor is common.
            IDamageable target = attacker.GetComponent<IDamageable>();
            if (target != null)
            {
                combat.SetAttackTarget(target);
            }
        }
    }

    protected override void UpdateUnit()
    {
    }
    
    private void OnDestroy()
    {
        if (damageable != null)
        {
            damageable.OnDamageTakenHandler -= OnDamageTaken;
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
