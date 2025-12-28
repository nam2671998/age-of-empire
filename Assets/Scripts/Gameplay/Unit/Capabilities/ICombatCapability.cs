public interface ICombatCapability
{
    void SetAttackTarget(IDamageable target);
    void Attack(IDamageable target);
    float GetAttackDamage();
    float GetAttackRange();
    bool IsInRange(IDamageable target);
    bool CanAttack();
}

