using UnityEngine;

public interface IDistanceStrategy
{
    float GetOptimalDistance(float baseRange);
    float GetStoppingDistance(IDamageable target, float baseRange);
    bool ShouldMaintainDistance(IDamageable target, Vector3 currentPosition, float optimalDistance);
}

