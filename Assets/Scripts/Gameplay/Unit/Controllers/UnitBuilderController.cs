using UnityEngine;

public partial class UnitBuilderController : MonoBehaviour, IBuildCapability
{
    [SerializeField] private int buildingPower = 20;
    [SerializeField] private float buildCooldown = 1f;
    [SerializeField] private float buildStoppingDistance = 0.25f;
    [SerializeField] private UnitAnimatorController animator;

    private static readonly IUnitBuilderControllerState idleState = new UnitBuilderIdleState();
    private static readonly IUnitBuilderControllerState movingToTargetState = new UnitBuilderMovingToTargetState();
    private static readonly IUnitBuilderControllerState buildingState = new UnitBuilderBuildingState();

    private IBuildable currentTarget;
    private IUnitBuilderControllerState currentState;
    private float lastBuildTime;

    private Vector3 reservedBuildPosition;
    private bool hasReservedBuildPosition;
    private float lastReserveAttemptTime = float.NegativeInfinity;

    private CommandExecutor executor;
    private IMovementCapability movementOwner;

    public bool IsBuilding => currentState != null && currentState != idleState;
    private bool HasValidTarget => currentTarget != null && currentTarget.GetGameObject() != null;

    private void Awake()
    {
        TryGetComponent(out executor);
        TryGetComponent(out movementOwner);
        SetState(idleState);
    }

    public void SetBuildTarget(IBuildable target)
    {
        if (target == null || target.GetGameObject() == null || target.IsComplete())
        {
            StopBuilding();
            return;
        }

        if (TryGetComponent(out IStopAction s))
        {
            s.StopOtherActions();
        }

        currentTarget = target;
        lastBuildTime = Time.time - buildCooldown;
        hasReservedBuildPosition = false;
        lastReserveAttemptTime = float.NegativeInfinity;

        SetState(movingToTargetState);
    }

    public void Build(IBuildable target)
    {
        if (target != null && !ReferenceEquals(target, currentTarget))
        {
            currentTarget = target;
        }

        currentState?.Tick(this);
    }

    public bool CanBuild()
    {
        return Time.time >= lastBuildTime + buildCooldown;
    }

    public void StopBuilding()
    {
        movementOwner?.StopMovement();
        ReleaseReservation();
        currentTarget = null;
        animator?.TriggerIdle();
        SetState(idleState);
    }

    public bool IsInRange(IBuildable target)
    {
        if (target == null)
        {
            return false;
        }

        Vector3 position = transform.position;
        position.y = reservedBuildPosition.y;
        return Vector3.Distance(position, reservedBuildPosition) <= buildStoppingDistance;
    }

    private void SetState(IUnitBuilderControllerState nextState)
    {
        if (nextState == null || nextState == currentState)
        {
            return;
        }

        currentState?.Exit(this);
        currentState = nextState;
        currentState.Enter(this);
    }

    private bool TryReservePosition()
    {
        lastReserveAttemptTime = Time.time;

        if (!HasValidTarget)
        {
            return false;
        }

        if (hasReservedBuildPosition && executor != null)
        {
            currentTarget.ReleaseBuildPosition(movementOwner);
            hasReservedBuildPosition = false;
        }

        if (executor == null)
        {
            reservedBuildPosition = currentTarget.GetNearestBuildPosition(transform.position);
            return false;
        }

        bool reserved = currentTarget.TryReserveBuildPosition(executor, out Vector3 position);
        reservedBuildPosition = position;
        hasReservedBuildPosition = reserved;
        return reserved;
    }

    private void ReleaseReservation()
    {
        if (!hasReservedBuildPosition)
        {
            return;
        }

        if (executor == null || !HasValidTarget)
        {
            hasReservedBuildPosition = false;
            return;
        }

        currentTarget.ReleaseBuildPosition(movementOwner);
        hasReservedBuildPosition = false;
    }

    private void FaceTarget(IBuildable target)
    {
        movementOwner.SetAutoRotate(false);
        Vector3 direction = target.GetGameObject().transform.position - transform.position;
        direction.y = 0f;

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f * Time.deltaTime);
        }
    }
}

