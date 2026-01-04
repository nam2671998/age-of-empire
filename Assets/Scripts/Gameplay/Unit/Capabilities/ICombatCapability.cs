public interface ICombatCapability
{
    void SetAttackTarget(IDamageable target);
    IDamageable CurrentTarget { get; }
    void SetAttackStrategy(IAttackStrategy strategy);
    void SetDistanceStrategy(IDistanceStrategy strategy);
    void StopAttacking();
    void TickAttack();
    bool IsAttackFinished();
}

