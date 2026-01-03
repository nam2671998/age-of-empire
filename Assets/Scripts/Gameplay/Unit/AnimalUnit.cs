using UnityEngine;

[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(UnitCombatController))]
[RequireComponent(typeof(UnitMovementController))]
[RequireComponent(typeof(UnitActionStateController))]
public class AnimalUnit : MonoBehaviour, IStopAction
{
    [SerializeField] private GameObject meatPrefab;
    private ICombatCapability combat;
    private Damageable damageable;
    private UnitMovementController movement;
    private UnitActionStateController stateManager;
    private UnitAnimatorController animator;
    
    void Awake()
    {
        InitializeComponents();
        InitializeUnit();
    }
    
    void Update()
    {
        if (movement != null && movement.IsMoving)
        {
            movement.UpdateMovement();
        }
        
        if (stateManager != null)
        {
            stateManager.UpdateState();
        }
        
        if (animator != null)
        {
            float speed = movement != null ? movement.GetCurrentSpeed() : 0f;
            animator.UpdateState(stateManager.CurrentState, speed);
        }
    }

    private void InitializeComponents()
    {
        TryGetComponent(out movement);
        TryGetComponent(out stateManager);
        TryGetComponent(out animator);
        TryGetComponent(out combat);
        TryGetComponent(out damageable);
    }

    private void InitializeUnit()
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
            damageable.OnDeath += OnDeath;
        }
    }

    private void OnDeath(Unit attacker)
    {
        ObjectPool.Spawn(meatPrefab, transform.position, Quaternion.identity);
        gameObject.SetActive(false);
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
