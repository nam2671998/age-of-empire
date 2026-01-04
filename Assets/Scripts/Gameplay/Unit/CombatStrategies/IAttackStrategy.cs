using UnityEngine;

public interface IAttackStrategy
{
    void ExecuteAttack(IDamageable target, int damage);
    bool CanAttack(float lastAttackTime, float attackCooldown);
    bool TryFindNearbyTarget(Faction attackerFaction, Vector3 origin, float searchRadius, out IDamageable target);
}

