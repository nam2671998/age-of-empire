public interface ICombatCapability
{
    void SetAttackTarget(IAttackable target);
    void Attack(IAttackable target);
    float GetAttackDamage();
    float GetAttackRange();
    bool IsInRange(IAttackable target);
    bool CanAttack();
}

