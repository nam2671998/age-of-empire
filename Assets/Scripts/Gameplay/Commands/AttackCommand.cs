using UnityEngine;

public class AttackCommand : BaseCommand
{
    private IDamageable target;
    private float attackRange = 2f;
    private float attackCooldown = 1f;
    private float lastAttackTime = 0f;
    private float chaseRange = 20f;
    
    public AttackCommand(IDamageable target, float attackRange = 2f, float attackCooldown = 1f)
    {
        this.target = target;
        this.attackRange = attackRange;
        this.attackCooldown = attackCooldown;
    }
    
    public override void Execute(CommandExecutor executor)
    {
        if (executor == null)
        {
            Debug.LogError("AttackCommand: Executor is null");
            return;
        }
        
        if (target == null || target.GetGameObject() == null)
        {
            Debug.LogWarning("AttackCommand: Target is null or destroyed");
            Complete();
            return;
        }
        
        if (executor.TryGetCapability(out ICombatCapability combatUnit))
        {
            combatUnit.SetAttackTarget(target);
        }
        else
        {
            Debug.LogWarning($"AttackCommand: ICombatUnit capability not found");
            Complete();
        }
    }
    
    protected override void OnUpdate(CommandExecutor executor)
    {
        if (executor == null)
        {
            Complete();
            return;
        }
        
        if (target == null || target.GetGameObject() == null || target.IsDestroyed())
        {
            Complete();
            return;
        }
        
        if (!executor.TryGetCapability(out ICombatCapability combatUnit))
        {
            Complete();
            return;
        }
        
        if (!executor.TryGetCapability(out IMovementCapability movement))
        {
            Complete();
            return;
        }
        
        Vector3 targetPosition = target.GetPosition();
        float distanceToTarget = Vector3.Distance(movement.transform.position, targetPosition);
        
        if (distanceToTarget > chaseRange)
        {
            Complete();
            return;
        }
        
        if (combatUnit.IsInRange(target))
        {
            if (combatUnit.CanAttack())
            {
                combatUnit.Attack(target);
            }
        }
        else
        {
            movement.MoveTo(targetPosition, combatUnit.GetAttackRange() * 0.8f);
        }
    }
    
    public override string GetDescription()
    {
        if (target == null || target.GetGameObject() == null)
            return "Attack (target destroyed)";
            
        return $"Attack {target.GetGameObject().name}";
    }
    
    public IDamageable GetTarget()
    {
        return target;
    }
}

