using UnityEngine;

public class ProjectileAttackStrategy : IAttackStrategy, IDistanceStrategy, INearbyTargetFinder
{
    private static readonly Collider[] overlapResults = new Collider[64];

    private Projectile projectilePrefab;
    private Transform projectileSpawnPoint;
    private float rangeMultiplier = 1.5f;
    private float minDistance = 3f;
    private float projectileSpawnDelay;

    public ProjectileAttackStrategy(Projectile projectilePrefab, Transform projectileSpawnPoint, float projectileSpawnDelay, float rangeMultiplier = 1.5f, float minDistance = 3f)
    {
        this.projectilePrefab = projectilePrefab;
        this.projectileSpawnPoint = projectileSpawnPoint;
        this.rangeMultiplier = rangeMultiplier;
        this.minDistance = minDistance;
        this.projectileSpawnDelay = projectileSpawnDelay;
    }

    public async void ExecuteAttack(IDamageable target, int damage, Transform attackerTransform)
    {
        if (target == null || target.IsDestroyed())
            return;

        if (attackerTransform == null)
            return;

        await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(projectileSpawnDelay * 1000));
        
        if (target.IsDestroyed())
            return;

        if (attackerTransform == null)
            return;
        
        Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : attackerTransform.position;
        FaceTarget(target, attackerTransform);
        Projectile projectile = ObjectPool.Spawn(projectilePrefab, spawnPos, Quaternion.identity);
        Unit attackerUnit = attackerTransform.GetComponent<Unit>();
        projectile.Initialize(target, damage, attackerUnit);
    }

    public bool TryFindNearbyTarget(Faction attackerFaction, Vector3 origin, float searchRadius, out IDamageable target)
    {
        target = null;
        int count = Physics.OverlapSphereNonAlloc(origin, searchRadius, overlapResults);

        float bestDistanceSqr = float.PositiveInfinity;
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
            if (distSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distSqr;
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
        return baseRange * rangeMultiplier;
    }

    public bool ShouldMaintainDistance(Vector3 targetPosition, Vector3 currentPosition, float optimalDistance)
    {
        float distance = Vector3.Distance(currentPosition, targetPosition);
        return distance < minDistance || distance > optimalDistance * 1.2f;
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
