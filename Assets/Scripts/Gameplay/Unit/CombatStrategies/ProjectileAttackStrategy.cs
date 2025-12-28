using UnityEngine;

public class ProjectileAttackStrategy : IAttackStrategy, IDistanceStrategy
{
    private Projectile projectilePrefab;
    private Transform projectileSpawnPoint;
    private float rangeMultiplier = 1.5f;
    private float minDistance = 3f;

    public ProjectileAttackStrategy(Projectile projectilePrefab, Transform projectileSpawnPoint, float rangeMultiplier = 1.5f, float minDistance = 3f)
    {
        this.projectilePrefab = projectilePrefab;
        this.projectileSpawnPoint = projectileSpawnPoint;
        this.rangeMultiplier = rangeMultiplier;
        this.minDistance = minDistance;
    }

    public void ExecuteAttack(IDamageable target, float damage, Transform attackerTransform)
    {
        if (target == null || target.IsDestroyed())
            return;

        FaceTarget(target, attackerTransform);

        Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : attackerTransform.position;
        Projectile projectile = Object.Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        
        Unit attackerUnit = attackerTransform.GetComponent<Unit>();
        projectile.Initialize(target, damage, attackerUnit);
    }

    public bool CanAttack(float lastAttackTime, float attackCooldown)
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    public float GetOptimalDistance(float baseRange)
    {
        return baseRange * rangeMultiplier;
    }

    public Vector3 CalculatePosition(Vector3 targetPosition, Vector3 currentPosition, float optimalDistance)
    {
        Vector3 direction = (targetPosition - currentPosition);
        direction.y = 0f;
        
        float currentDistance = direction.magnitude;
        
        if (currentDistance >= minDistance && currentDistance <= optimalDistance)
        {
            return currentPosition;
        }
        
        if (currentDistance < minDistance)
        {
            return currentPosition - direction.normalized * (minDistance - currentDistance);
        }
        
        return targetPosition - direction.normalized * optimalDistance;
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
