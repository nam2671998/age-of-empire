using UnityEngine;

public interface IDistanceStrategy
{
    float GetOptimalDistance(float baseRange);
    Vector3 CalculatePosition(Vector3 targetPosition, Vector3 currentPosition, float optimalDistance);
    bool ShouldMaintainDistance(Vector3 targetPosition, Vector3 currentPosition, float optimalDistance);
}

