using UnityEngine;

public interface IAttackStrategy
{
    void ExecuteAttack(IAttackable target, float damage, Transform attackerTransform);
    bool CanAttack(float lastAttackTime, float attackCooldown);
}

