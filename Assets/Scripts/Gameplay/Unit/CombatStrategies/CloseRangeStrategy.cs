using UnityEngine;

public class CloseRangeStrategy : IAttackStrategy, IDistanceStrategy
{
    private float attackWindUpDuration;
    private readonly float surfacePadding;

    public CloseRangeStrategy(float attackWindUpDuration)
    {
        this.attackWindUpDuration = attackWindUpDuration;
        surfacePadding = 0.1f;
    }

    private static readonly Collider[] overlapResults = new Collider[64];

    public async void ExecuteAttack(IDamageable target, int damage)
    {
        if (target == null || target.IsDestroyed())
            return;

        await System.Threading.Tasks.Task.Delay((int)(attackWindUpDuration * 1000));
        target.TakeDamage(damage);
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
    
    public float GetStoppingDistance(IDamageable target, float baseRange)
    {
        if (target == null)
        {
            return baseRange;
        }

        float desiredSurfaceDistance = GetOptimalDistance(baseRange);

        Collider col = target.HitCollider;
        if (col == null)
        {
            return desiredSurfaceDistance;
        }

        Vector3 extents = col.bounds.extents;
        float radius = Mathf.Max(extents.x, extents.z);
        return Mathf.Max(0f, desiredSurfaceDistance + radius + surfacePadding);
    }
    
    public bool ShouldMaintainDistance(IDamageable target, Vector3 currentPosition, float optimalDistance)
    {
        if (target == null)
        {
            return false;
        }

        Collider col = target.HitCollider;
        Vector3 origin = currentPosition;
        origin.y = 0f;

        Vector3 targetPoint;
        if (col != null)
        {
            Vector3 closest = col.ClosestPoint(origin);
            targetPoint = new Vector3(closest.x, 0f, closest.z);
        }
        else
        {
            Vector3 p = target.GetPosition();
            targetPoint = new Vector3(p.x, 0f, p.z);
        }

        float distance = Vector3.Distance(origin, targetPoint);
        return distance > optimalDistance * 1.2f;
    }
}

