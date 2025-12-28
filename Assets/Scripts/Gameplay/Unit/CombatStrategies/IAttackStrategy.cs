using UnityEngine;

public interface IAttackStrategy
{
    void ExecuteAttack(IDamageable target, float damage, Transform attackerTransform);
    bool CanAttack(float lastAttackTime, float attackCooldown);
}

