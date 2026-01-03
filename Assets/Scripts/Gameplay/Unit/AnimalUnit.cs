using UnityEngine;

[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(UnitCombatController))]
[RequireComponent(typeof(UnitMovementController))]
[RequireComponent(typeof(UnitActionStateController))]
public class AnimalUnit : MonoBehaviour, IStopAction
{
    [SerializeField] private GameObject meatPrefab;
    [SerializeField] private float attackWindUpDuration = 0;
    private ICombatCapability combat;
    private Damageable damageable;
    private UnitMovementController movement;
    private UnitActionStateController stateManager;
    private UnitAnimatorController animator;
    private static readonly Collider[] overlapResults = new Collider[64];
    
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
            CloseRangeStrategy strategy = new CloseRangeStrategy(attackWindUpDuration);
            combat.SetAttackStrategy(strategy);
            combat.SetDistanceStrategy(strategy);
        }

        if (damageable != null)
        {
            damageable.OnDamageTakenHandler += OnDamageTaken;
            damageable.OnDeath += OnDeath;
        }
    }

    private void OnDeath()
    {
        ObjectPool.Spawn(meatPrefab, transform.position, Quaternion.identity);
        gameObject.SetActive(false);
    }

    private void OnDamageTaken()
    {
        if (combat != null)
        {
            // Retaliate if not already attacking someone else? Or switch target?
            // Switch target to latest aggressor is common.
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, 20, overlapResults, LayerMask.GetMask("Unit"));
            for (int i = 0; i < hitCount; i++)
            {
                Collider col = overlapResults[i];
                if (col.gameObject.TryGetComponent(out IDamageable target))
                {
                    combat.SetAttackTarget(target);
                }
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
