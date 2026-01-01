using UnityEngine;

public class CloseRangeStrategy : IAttackStrategy, IDistanceStrategy, INearbyTargetFinder
{
    private static readonly Collider[] overlapResults = new Collider[64];

    public void ExecuteAttack(IDamageable target, int damage, Transform attackerTransform)
    {
        if (target == null || target.IsDestroyed())
            return;
        
        FaceTarget(target, attackerTransform);
        
        if (attackerTransform.TryGetComponent(out Unit unit))
        {
            target.TakeDamage(damage, unit);
        }
    }

    public bool TryFindNearbyTarget(Faction attackerFaction, Vector3 origin, float searchRadius, out IDamageable target)
    {
        target = null;
        int count = Physics.OverlapSphereNonAlloc(origin, searchRadius, overlapResults);

        float shortestDistance = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            Collider col = overlapResults[i];
            if (col == null)
            {
                continue;
            }

            IDamageable candidate = col.GetComponentInParent<IDamageable>();
            if (candidate == null || candidate.IsDestroyed() || candidate.GetGameObject() == null)
            {
                continue;
            }

            if (attackerFaction == candidate.Faction)
            {
                continue;
            }

            Vector3 delta = candidate.GetPosition() - origin;
            float distSqr = delta.sqrMagnitude;
            if (distSqr < shortestDistance)
            {
                shortestDistance = distSqr;
                target = candidate;
            }
        }

        return target != null;
    }
    
    public bool CanAttack(float lastAttackTime, float attackCooldown)
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }
    
    public float GetOptimalDistance(float baseRange)
    {
        return baseRange * 0.8f;
    }
    
    public bool ShouldMaintainDistance(Vector3 targetPosition, Vector3 currentPosition, float optimalDistance)
    {
        float distance = Vector3.Distance(currentPosition, targetPosition);
        return distance > optimalDistance * 1.2f;
    }
    
    private void FaceTarget(IDamageable target, Transform attackerTransform)
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

