using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovementController : MonoBehaviour, IMovementCapability, IGridEntity
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private UnitAnimatorController animatorController;
    [SerializeField] private Transform moveTargetTransform;
    
    private Vector3 moveTarget;
    private bool isMoving = false;
    
    public bool IsMoving => isMoving;
    public float MoveSpeed => moveSpeed;

    public Transform GetTransform()
    {
        try
        {
            return transform;
        }
        catch
        {
            return null;
        }
    }

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (animatorController == null)
            animatorController = GetComponent<UnitAnimatorController>();

        if (agent == null)
        {
            Debug.LogError($"{nameof(UnitMovementController)} requires a {nameof(NavMeshAgent)} component.", this);
            enabled = false;
            return;
        }

        ApplyAgentSettings();
    }

    private void OnDisable()
    {
        // Free grid reservations when unit is destroyed
        if (GridManager.Instance != null)
        {
            GridManager.Instance.FreeUnitReservation(this);
        }
    }

    private void ApplyAgentSettings()
    {
        if (agent == null)
            return;

        agent.speed = moveSpeed;
        agent.angularSpeed = rotationSpeed;
        agent.stoppingDistance = 0;
        agent.updateRotation = true;
    }
    
    public void MoveTo(Vector3 targetPosition, float stoppingDistance = 0)
    {
        // ApplyAgentSettings();
        moveTarget = targetPosition;
        agent.stoppingDistance = stoppingDistance;

        if (agent == null || !agent.isOnNavMesh)
        {
            isMoving = false;
            return;
        }

        if (!TrySetDestination(moveTarget))
        {
            isMoving = false;
            return;
        }

        if (agent.remainingDistance < stoppingDistance)
        {
            isMoving = false;
            return;
        }

        agent.isStopped = false;
        if (!isMoving)
        {
            animatorController.TriggerMove();
            SetAutoRotate(true);
        }
        isMoving = true;
    }
    
    public void Stop()
    {
        isMoving = false;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }
    
    public void StopMovement()
    {
        Stop();
    }
    
    public void UpdateMovement()
    {
        if (agent == null)
        {
            isMoving = false;
            return;
        }

        // ApplyAgentSettings();

        if (!isMoving)
            return;

        if (!agent.isOnNavMesh)
        {
            isMoving = false;
            return;
        }

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid || (!agent.pathPending && !agent.hasPath))
        {
            isMoving = false;
            return;
        }

        if (agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            Debug.LogError("Could not reach destination (partial path).", this);
            Stop();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.01f))
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.0001f)
                isMoving = false;
        }
    }
    
    public float GetCurrentSpeed()
    {
        if (agent == null)
            return 0f;

        return isMoving ? agent.velocity.magnitude : 0f;
    }

    private bool TrySetDestination(Vector3 destination)
    {
        if (agent == null)
            return false;

        NavMeshPath path = new NavMeshPath();
        bool hasPath = agent.CalculatePath(destination, path);

        if (!hasPath || path.status == NavMeshPathStatus.PathInvalid)
        {
            Debug.LogError("Could not calculate a path to destination.", this);
            agent.ResetPath();
            return false;
        }

        if (path.status == NavMeshPathStatus.PathPartial)
        {
            Debug.LogError("Destination is unreachable (partial path).", this);
            agent.ResetPath();
            return false;
        }

        agent.SetPath(path);
        return true;
    }

    public void SetAutoRotate(bool auto)
    {
        agent.updateRotation = auto;
    }
}

