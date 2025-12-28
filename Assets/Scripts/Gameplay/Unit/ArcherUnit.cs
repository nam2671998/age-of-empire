using UnityEngine;

[RequireComponent(typeof(UnitCombatController))]
[RequireComponent(typeof(Damageable))]
public class ArcherUnit : Unit, IStopAction
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    
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
            if (projectileSpawnPoint == null) projectileSpawnPoint = transform;
            
            ProjectileAttackStrategy strategy = new ProjectileAttackStrategy(projectilePrefab, projectileSpawnPoint);
            combat.SetAttackStrategy(strategy);
            combat.SetDistanceStrategy(strategy);
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
