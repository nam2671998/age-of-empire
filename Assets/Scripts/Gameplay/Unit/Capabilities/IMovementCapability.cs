using UnityEngine;

public interface IMovementCapability
{
    void MoveTo(Vector3 targetPosition, float stoppingDistance = 0.5f);
    void StopMovement();
    bool IsMoving { get; }
    float MoveSpeed { get; }
    Transform transform { get; }
}

