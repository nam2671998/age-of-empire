using UnityEngine;

public interface IAttackStrategy
{
    void ExecuteAttack(IDamageable target, int damage, Transform attackerTransform);
    bool CanAttack(float lastAttackTime, float attackCooldown);
}

