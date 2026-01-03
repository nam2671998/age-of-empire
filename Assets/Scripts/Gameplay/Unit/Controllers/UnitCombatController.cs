using UnityEngine;

public class UnitCombatController : MonoBehaviour, ICombatCapability, IFactionOwner
{
    private static readonly Collider[] overlapResults = new Collider[64];

    [SerializeField] private Faction faction = Faction.Player1;

    [Header("Combat Settings")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float chaseRange = 20f;

    private IMovementCapability movementOwner;
    private IDamageable attackTarget;
    private float lastAttackTime;
    private bool isAttacking;
    private Vector3 lastTargetPosition;
    
    private IAttackStrategy attackStrategy;
    private IDistanceStrategy distanceStrategy;
    private UnitAnimatorController animator;
    
    public bool IsAttacking => isAttacking;
    public Faction Faction
    {
        get => faction;
        set => faction = value;
    }
    public IDamageable CurrentTarget => attackTarget;
    
    private void Awake()
    {
        TryGetComponent(out animator);
        TryGetComponent(out movementOwner);
    }

    public void SetAttackStrategy(IAttackStrategy strategy)
    {
        attackStrategy = strategy;
    }
    
    public void SetDistanceStrategy(IDistanceStrategy strategy)
    {
        distanceStrategy = strategy;
    }
    
    public void SetAttackTarget(IDamageable target)
    {
        if (target != null && !CanAttackTarget(target))
        {
            Debug.Log($"{gameObject.name} cannot attack {target.GetGameObject().name} - same faction ({faction})");
            return;
        }
        
        if (target != null && TryGetComponent(out IStopAction s))
        {
            s.StopOtherActions();
        }
        
        attackTarget = target;
        
        if (target != null && target.GetGameObject() != null && !target.IsDestroyed() && TryGetComponent(out IMovementCapability movementOwner))
        {
            float optimalDistance = distanceStrategy != null
                ? distanceStrategy.GetOptimalDistance(attackRange)
                : attackRange * 0.8f;
            movementOwner.MoveTo(target.GetPosition(), optimalDistance);
            isAttacking = true;
        }
    }
    
    public void Attack(IDamageable target)
    {
        if (target == null || target.IsDestroyed())
        {
            attackTarget = null;
            isAttacking = false;
            return;
        }
        
        if (!CanAttack())
        {
            return;
        }
        
        if (!CanAttackTarget(target))
        {
            return;
        }
        
        if (attackStrategy == null)
        {
            return;
        }
        
        attackStrategy.ExecuteAttack(target, attackDamage, transform);
        FaceTarget(target);
        lastAttackTime = Time.time;
        lastTargetPosition = target.GetPosition();
        isAttacking = true;

        if (animator != null)
        {
            animator.TriggerAttack();
        }
    }

    public bool IsInRange(IDamageable target)
    {
        if (target == null)
            return false;
        
        float distance = Vector3.Distance(transform.position, target.GetPosition());
        
        if (distanceStrategy != null)
        {
            float optimalDistance = distanceStrategy.GetOptimalDistance(attackRange);
            return !distanceStrategy.ShouldMaintainDistance(target.GetPosition(), transform.position, optimalDistance);
        }
        
        return distance <= attackRange;
    }

    public bool CanAttack()
    {
        if (attackStrategy != null)
        {
            return attackStrategy.CanAttack(lastAttackTime, attackCooldown);
        }

        return Time.time >= lastAttackTime + attackCooldown;
    }

    public void StartAttack(IDamageable target)
    {
        SetAttackTarget(target);
        if (target == null || target.GetGameObject() == null || target.IsDestroyed())
        {
            StopAttacking();
        }
        else
        {
            isAttacking = true;
        }
    }

    public void TickAttack()
    {
        if (attackTarget == null || attackTarget.GetGameObject() == null || attackTarget.IsDestroyed())
        {
            if (TryFindNearbyTarget(chaseRange, out IDamageable nearby))
            {
                SetAttackTarget(nearby);
            }
            else
            {
                StopAttacking();
                return;
            }
        }

        if (attackTarget == null)
        {
            StopAttacking();
            return;
        }

        Vector3 targetPosition = attackTarget.GetPosition();
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        if (distanceToTarget > chaseRange)
        {
            StopAttacking();
            return;
        }

        if (IsInRange(attackTarget))
        {
            if (CanAttack())
            {
                Attack(attackTarget);
            }
            return;
        }

        if (!TryGetComponent(out IMovementCapability movementOwner))
        {
            StopAttacking();
            return;
        }

        float optimalDistance = distanceStrategy != null
            ? distanceStrategy.GetOptimalDistance(attackRange)
            : attackRange * 0.8f;

        if (!movementOwner.IsMoving)
        {
            movementOwner.MoveTo(targetPosition, optimalDistance);
        }
    }

    public bool IsAttackFinished()
    {
        return attackTarget == null || attackTarget.IsDestroyed();
    }

    public void StopAttacking()
    {
        isAttacking = false;
        attackTarget = null;
    }

    private bool CanAttackTarget(IDamageable target)
    {
        if (target == null)
        {
            return false;
        }

        return faction != target.Faction;
    }

    private bool TryFindNearbyTarget(float searchRadius, out IDamageable target)
    {
        target = null;
        if (searchRadius <= 0f)
        {
            return false;
        }

        if (attackStrategy is INearbyTargetFinder finder && finder.TryFindNearbyTarget(faction, lastTargetPosition, searchRadius, out target))
        {
            return true;
        }

        Vector3 origin = transform.position;
        int count = Physics.OverlapSphereNonAlloc(origin, searchRadius, overlapResults);

        float shortestDistance = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            Collider col = overlapResults[i];
            if (col == null)
            {
                continue;
            }

            IDamageable candidate = col.GetComponentInParent<IDamageable>();
            if (candidate == null || candidate.IsDestroyed() || candidate.GetGameObject() == null)
            {
                continue;
            }

            if (candidate.GetGameObject() == gameObject)
            {
                continue;
            }

            if (candidate.Faction == faction)
            {
                continue;
            }

            Vector3 delta = candidate.GetPosition() - origin;
            float distSqr = delta.sqrMagnitude;
            if (distSqr < shortestDistance)
            {
                shortestDistance = distSqr;
                target = candidate;
            }
        }

        return target != null;
    }
    
    private void FaceTarget(IDamageable target)
    {
        movementOwner.SetAutoRotate(false);
        Vector3 direction = (target.GetPosition() - transform.position);
        direction.y = 0f;
        
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f * Time.deltaTime);
        }
    }
}

