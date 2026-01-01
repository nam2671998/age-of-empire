using UnityEngine;

public class AttackCommand : BaseCommand
{
    private readonly IDamageable target;
    private ICombatCapability combatant;
    
    public AttackCommand(IDamageable target)
    {
        this.target = target;
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
            Debug.LogError("AttackCommand: Target is null or destroyed");
            Complete();
            return;
        }
        
        if (!executor.TryGetCapability(out combatant))
        {
            Complete();
            return;
        }

        combatant.SetAttackTarget(target);
    }
    
    protected override void OnUpdate(CommandExecutor executor)
    {
        if (executor == null)
        {
            Complete();
            return;
        }

        if (combatant == null && !executor.TryGetCapability(out combatant))
        {
            Complete();
            return;
        }

        combatant.TickAttack();

        if (combatant.IsAttackFinished())
        {
            Complete();
        }
    }
    
    public override string GetDescription()
    {
        if (target == null || target.GetGameObject() == null)
        {
            return "Attack (target destroyed)";
        }

        return $"Attack {target.GetGameObject().name}";
    }

    protected override void OnCancel(CommandExecutor executor)
    {
        base.OnCancel(executor);
        if (executor != null && executor.TryGetComponent(out ICombatCapability combat))
        {
            combat.StopAttacking();
        }
    }
}

