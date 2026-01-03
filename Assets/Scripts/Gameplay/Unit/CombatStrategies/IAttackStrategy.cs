using UnityEngine;

public interface IAttackStrategy
{
    void ExecuteAttack(IDamageable target, int damage);
    bool CanAttack(float lastAttackTime, float attackCooldown);
}

