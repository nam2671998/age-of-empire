using UnityEngine;

public partial class UnitHarvesterController : MonoBehaviour, IHarvestCapability
{
    [SerializeField] private int harvestAmount = 1;
    [SerializeField] private float harvestRange = 1;
    [SerializeField] private float harvestCooldown = 1f;
    [SerializeField] private float findNextRadius = 10f;
    [SerializeField] private LayerMask harvestableResourceMask;
    [SerializeField] private UnitAnimatorController animator;

    private static readonly IUnitHarvesterControllerState idleState = new UnitHarvesterIdleState();
    private static readonly IUnitHarvesterControllerState movingToTargetState = new UnitHarvesterMovingToTargetState();
    private static readonly IUnitHarvesterControllerState harvestingState = new UnitHarvesterHarvestingState();
    private static readonly IUnitHarvesterControllerState searchingState = new UnitHarvesterSearchingState();

    private float lastHarvestTime = 0f;
    private IHarvestable currentTarget;
    private ResourceType currentResourceType;
    private IUnitHarvesterControllerState currentState;
    private IMovementCapability movement;

    public bool IsHarvesting => currentState != null && currentState != idleState;
    private bool HasValidTarget => currentTarget != null && currentTarget.GetGameObject() != null;
    private bool IsTargetDepleted => currentTarget != null && currentTarget.IsDepleted();

    private void Awake()
    {
        TryGetComponent(out movement);
        SetState(idleState);
    }

    public void StartHarvest(IHarvestable target)
    {
        if (target == null || target.GetGameObject() == null || target.IsDepleted())
        {
            StopHarvest();
            return;
        }

        if (TryGetComponent(out IStopAction s))
        {
            s.StopOtherActions();
        }

        currentTarget = target;
        currentResourceType = target.GetResourceType();
        lastHarvestTime = Time.time - harvestCooldown;

        SetState(movingToTargetState);
    }

    public void TickHarvest()
    {
        currentState?.Tick(this);
    }

    private bool CanHarvest()
    {
        return Time.time >= lastHarvestTime + harvestCooldown;
    }

    public void StopHarvest()
    {
        movement?.StopMovement();
        currentTarget = null;
        animator.TriggerIdle();
        SetState(idleState);
    }

    private int Harvest(IHarvestable target)
    {
        if (target == null || target.IsDepleted())
        {
            return 0;
        }
        
        if (!CanHarvest())
        {
            return 0;
        }
        
        FaceTarget(target);
        int harvested = target.Harvest(harvestAmount);
        lastHarvestTime = Time.time;

        if (animator != null)
        {
            if (harvested > 0)
            {
                animator.TriggerHarvest();
            }
            else
            {
                animator.TriggerIdle();
            }
        }
        
        return harvested;
    }

    private bool IsInRange(IHarvestable target)
    {
        if (target == null)
            return false;
        
        float distance = Vector3.Distance(transform.position, target.GetHarvestPosition());
        return distance <= harvestRange;
    }

    private void SetCurrentTarget(IHarvestable target)
    {
        if (target == null)
            return;

        currentTarget = target;
        currentResourceType = target.GetResourceType();
    }

    private void SetState(IUnitHarvesterControllerState nextState)
    {
        if (nextState == null || nextState == currentState)
            return;

        currentState?.Exit(this);
        currentState = nextState;
        currentState.Enter(this);
    }

    private void FaceTarget(IHarvestable target)
    {
        Vector3 direction = (target.GetPosition() - transform.position);
        direction.y = 0f;
        
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f * Time.deltaTime);
        }
    }

    private void Idle()
    {
        animator.TriggerIdle();
    }
}
