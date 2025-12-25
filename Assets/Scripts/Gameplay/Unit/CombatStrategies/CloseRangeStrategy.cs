using UnityEngine;

public class CloseRangeStrategy : IAttackStrategy, IDistanceStrategy
{
    public void ExecuteAttack(IAttackable target, float damage, Transform attackerTransform)
    {
        if (target == null || target.IsDestroyed())
            return;
        
        FaceTarget(target, attackerTransform);
        
        if (attackerTransform.TryGetComponent(out Unit unit))
        {
            target.TakeDamage(damage, unit);
        }
    }
    
    public bool CanAttack(float lastAttackTime, float attackCooldown)
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }
    
    public float GetOptimalDistance(float baseRange)
    {
        return baseRange * 0.8f;
    }
    
    public Vector3 CalculatePosition(Vector3 targetPosition, Vector3 currentPosition, float optimalDistance)
    {
        Vector3 direction = (targetPosition - currentPosition);
        direction.y = 0f;
        
        if (direction.magnitude <= optimalDistance)
        {
            return currentPosition;
        }
        
        return targetPosition - direction.normalized * optimalDistance;
    }
    
    public bool ShouldMaintainDistance(Vector3 targetPosition, Vector3 currentPosition, float optimalDistance)
    {
        float distance = Vector3.Distance(currentPosition, targetPosition);
        return distance > optimalDistance * 1.2f;
    }
    
    private void FaceTarget(IAttackable target, Transform attackerTransform)
    {
        Vector3 direction = (target.GetPosition() - attackerTransform.position);
        direction.y = 0f;
        
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            attackerTransform.rotation = Quaternion.RotateTowards(attackerTransform.rotation, targetRotation, 360f * Time.deltaTime);
        }
    }
}

