using UnityEngine;

public class UnitCombatController : MonoBehaviour, ICombatCapability
{
    [Header("Combat Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    
    private IAttackable attackTarget;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private bool attackExecutedThisFrame = false;
    
    private IAttackStrategy attackStrategy;
    private IDistanceStrategy distanceStrategy;
    
    public IAttackable AttackTarget => attackTarget;
    public bool IsAttacking => isAttacking;
    
    public bool AttackExecutedThisFrame => attackExecutedThisFrame;
    
    void LateUpdate()
    {
        attackExecutedThisFrame = false;
    }
    
    public void OnLateUpdate()
    {
        attackExecutedThisFrame = false;
    }
    public float AttackDamage => attackDamage;
    public float AttackRange => attackRange;
    
    public float GetAttackDamage() => attackDamage;
    public float GetAttackRange() => attackRange;
    
    public void SetAttackStrategy(IAttackStrategy strategy)
    {
        attackStrategy = strategy;
    }
    
    public void SetDistanceStrategy(IDistanceStrategy strategy)
    {
        distanceStrategy = strategy;
    }
    
    public void SetTarget(IAttackable target)
    {
        attackTarget = target;
    }
    
    public void SetAttackTarget(IAttackable target)
    {
        if (target != null && !CanAttackTarget(target))
        {
            if (TryGetComponent(out IFactionOwner factionOwner))
            {
                Debug.LogWarning($"{gameObject.name} cannot attack {target.GetGameObject().name} - same faction ({factionOwner.Faction})");
            }
            return;
        }
        
        if (target != null && TryGetComponent(out SettlerUnit settler))
        {
            settler.StopOtherActions();
        }
        
        SetTarget(target);
        
        if (target != null && target.GetGameObject() != null && TryGetComponent(out IMovementCapability movement))
        {
            float optimalDistance = distanceStrategy != null 
                ? distanceStrategy.GetOptimalDistance(attackRange) 
                : attackRange * 0.8f;
            movement.MoveTo(target.GetPosition(), optimalDistance);
        }
    }
    
    public bool CanAttack()
    {
        if (attackStrategy != null)
        {
            return attackStrategy.CanAttack(lastAttackTime, attackCooldown);
        }
        return Time.time >= lastAttackTime + attackCooldown;
    }
    
    public void Attack(IAttackable target)
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
        
        if (attackStrategy != null)
        {
            attackStrategy.ExecuteAttack(target, attackDamage, transform);
        }
        else
        {
            FaceTarget(target);
            if (TryGetComponent(out Unit unit))
            {
                target.TakeDamage(attackDamage, unit);
            }
        }
        
        lastAttackTime = Time.time;
        isAttacking = true;
        attackExecutedThisFrame = true;
    }
    
    public bool CanAttackTarget(IAttackable target)
    {
        if (target == null)
            return false;
        
        if (!TryGetComponent(out IFactionOwner attacker))
            return false;
        
        Faction attackerFaction = attacker.Faction;
        Faction targetFaction = target.GetFaction();
        
        return attackerFaction != targetFaction;
    }
    
    public void StopAttacking()
    {
        isAttacking = false;
        attackTarget = null;
    }
    
    public bool IsInRange(IAttackable target)
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
    
    private void FaceTarget(IAttackable target)
    {
        Vector3 direction = (target.GetPosition() - transform.position);
        direction.y = 0f;
        
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f * Time.deltaTime);
        }
    }
}

