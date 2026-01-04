using UnityEngine;

/// <summary>
/// Control an unit's combat loop: keep a target, chase into range, and fire attacks on cooldown.
/// </summary>
/// <remarks>
/// <see cref="SetAttackTarget"/> sets a target and marks combat as active
/// <see cref="TickAttack"/> is basically Update()
/// <see cref="Attack"/> performs the actual damage via the configured strategy.
/// Actual damage is delegated to <see cref="IAttackStrategy"/>.
/// Spacing while moving is handled by <see cref="IDistanceStrategy"/> when present.
/// </remarks>
public class UnitCombatController : MonoBehaviour, ICombatCapability, IFactionOwner
{
    private static readonly Collider[] overlapResults = new Collider[64];

    [SerializeField] private Faction faction = Faction.Player1;

    [Header("Combat Settings")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackWindup = 0.5f;
    [SerializeField] private float autoFindRadius = 20f;
    [SerializeField] private float deathAnimationDuration = 1.33f;
    [SerializeField] private float chaseRetargetInterval = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioPlayer audioPlayer;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip deathClip;

    private IMovementCapability movementOwner;
    private IDamageable attackTarget;
    private float lastAttackTime;
    private bool isAttacking;
    private Vector3 lastTargetPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    private Damageable selfDamageable;
    
    private IAttackStrategy attackStrategy;
    private IDistanceStrategy distanceStrategy;
    private UnitAnimatorController animator;
    private float nextChaseRetargetTime = float.NegativeInfinity;
    
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
        TryGetComponent(out audioPlayer);
        if (TryGetComponent(out selfDamageable))
        {
            selfDamageable.OnDamageTakenHandler += Revenge;
            selfDamageable.OnDeath += OnDeath;
        }

    }
    
    private void OnDestroy()
    {
        if (selfDamageable != null)
        {
            selfDamageable.OnDamageTakenHandler -= Revenge;
            selfDamageable.OnDeath -= OnDeath;
        }
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
        // Prevent friendly fire at the moment a target is assigned.
        if (target != null && !CanAttackTarget(target))
        {
            if (!TryFindNearbyTarget(autoFindRadius, out target))
            {
                Debug.Log($"{gameObject.name} cannot attack {target.GetGameObject().name} - same faction ({faction})");
                return;
            }
        }
        
        // If we are switching to combat, stop other ongoing actions (e.g., harvesting/building).
        if (target != null && TryGetComponent(out IStopAction s))
        {
            s.StopOtherActions();
        }
        
        attackTarget = target;
        
        // Assigning a target also triggers chasing so the unit starts moving into range.
        if (target != null && target.GetGameObject() != null && !target.IsDestroyed() && movementOwner != null)
        {
            float stoppingDistance = distanceStrategy.GetStoppingDistance(target, attackRange);
            movementOwner.MoveTo(target.GetPosition(), stoppingDistance);
            nextChaseRetargetTime = Time.time + Mathf.Max(0f, chaseRetargetInterval);
            isAttacking = true;
        }
    }
    
    // Attack() is the "apply damage" step. Target selection/chasing happens elsewhere.
    public void Attack(IDamageable target)
    {
        if (target == null || target.IsDestroyed())
        {
            attackTarget = null;
            isAttacking = false;
            return;
        }
        
        if (!CanAttackTarget(target))
        {
            return;
        }
        
        if (!CanAttack())
        {
            return;
        }

        if (audioPlayer != null)
        {
            audioPlayer.PlayOneShot(attackClip);
        }

        attackStrategy.ExecuteAttack(target, attackDamage);
        lastAttackTime = Time.time;
        // Used as a hint for reacquiring nearby targets after losing the current one.
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
        
        float optimalDistance = distanceStrategy.GetOptimalDistance(attackRange);
        return !distanceStrategy.ShouldMaintainDistance(target, transform.position, optimalDistance);
    }

    public bool CanAttack()
    {
        return attackStrategy.CanAttack(lastAttackTime, attackCooldown);
    }

    // Called repeatedly while attacking to handle reacquire/chase/attack execution.
    public void TickAttack()
    {
        if (attackTarget == null || attackTarget.GetGameObject() == null || attackTarget.IsDestroyed())
        {
            // If we lost the target, try to pick a new one nearby before giving up.
            if (TryFindNearbyTarget(autoFindRadius, out IDamageable nearby))
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

        if (IsInRange(attackTarget))
        {
            // In range: only attack when cooldown allows it.
            if (CanAttack())
            {
                Attack(attackTarget);
            }
        
            FaceTarget(attackTarget);
            return;
        }

        // Out of range: chase the target until we reach the desired spacing.
        if (movementOwner != null && (!movementOwner.IsMoving || Time.time >= nextChaseRetargetTime))
        {
            float stoppingDistance = distanceStrategy.GetStoppingDistance(attackTarget, attackRange);
            movementOwner.MoveTo(targetPosition, stoppingDistance);
            nextChaseRetargetTime = Time.time + Mathf.Max(0f, chaseRetargetInterval);
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
        Debug.Log("Stop Attack");
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
        if (CurrentTarget != null && !CurrentTarget.IsDestroyed())
        {
            return false;
        }
        
        if (searchRadius <= 0f)
        {
            return false;
        }

        return attackStrategy.TryFindNearbyTarget(faction, transform.position, searchRadius, out target) ||
               attackStrategy.TryFindNearbyTarget(faction, lastTargetPosition, searchRadius, out target);
    }
    
    private void FaceTarget(IDamageable target)
    {
        // Manual facing makes attacks feel responsive, especially when movement autorotation is disabled.
        movementOwner.SetAutoRotate(false);
        Vector3 direction = (target.GetPosition() - transform.position);
        direction.y = 0f;
        
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f * Time.deltaTime);
        }
    }

    private void Revenge()
    {
        if (TryGetComponent(out CommandExecutor commandExecutor))
        {
            commandExecutor.SetCommand(new AttackCommand(null));
        }
    }

    private async void OnDeath()
    {
        if (audioPlayer != null)
        {
            audioPlayer.PlayOneShot(deathClip);
        }

        animator.TriggerDeath();
        await System.Threading.Tasks.Task.Delay((int)(deathAnimationDuration * 1000));
        gameObject.SetActive(false);
    }
}

