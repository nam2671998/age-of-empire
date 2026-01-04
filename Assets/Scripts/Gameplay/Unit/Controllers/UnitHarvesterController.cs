using System.Collections.Generic;
using UnityEngine;

public partial class UnitHarvesterController : MonoBehaviour, IHarvestCapability
{
    [SerializeField] private int harvestAmount = 1;
    [SerializeField] private float harvestRange = 1;
    [SerializeField] private float harvestCooldown = 1f;
    [SerializeField] private float findNextRadius = 10f;
    [SerializeField] private LayerMask harvestableResourceMask;
    [SerializeField] private UnitAnimatorController animator;
    [SerializeField] private int inventoryCapacity = 10;
    [SerializeField] private VoidEventChannelSO onResourceInventoryChanged;

    private static readonly IUnitHarvesterControllerState idleState = new UnitHarvesterIdleState();
    private static readonly IUnitHarvesterControllerState movingToTargetState = new UnitHarvesterMovingToTargetState();
    private static readonly IUnitHarvesterControllerState harvestingState = new UnitHarvesterHarvestingState();
    private static readonly IUnitHarvesterControllerState searchingState = new UnitHarvesterSearchingState();
    private static readonly IUnitHarvesterControllerState movingToDepositState = new UnitHarvesterMovingToDepositState();
    private static readonly IUnitHarvesterControllerState depositingState = new UnitHarvesterDepositingState();

    private float lastHarvestTime = 0f;
    private Vector3 currentHarvestPosition = new Vector3(float.MaxValue, float.MaxValue);
    private IHarvestable currentTarget;
    private ResourceType currentResourceType;
    private IUnitHarvesterControllerState currentState;
    private IMovementCapability movementOwner;
    private IFactionOwner factionOwner;

    private readonly Dictionary<ResourceType, int> inventory = new Dictionary<ResourceType, int>(4);
    private int inventoryCount;
    private IResourceHolderConstruction depositTarget;

    public bool IsHarvesting => currentState != null && currentState != idleState;
    private bool HasValidTarget => currentTarget != null && currentTarget.GetGameObject() != null;
    private bool IsTargetDepleted => currentTarget != null && currentTarget.IsDepleted();
    private bool IsInventoryFull => inventoryCapacity > 0 && inventoryCount >= inventoryCapacity;
    private bool HasValidDepositTarget => depositTarget != null && depositTarget.GetGameObject() != null;

    private void Awake()
    {
        TryGetComponent(out movementOwner);
        TryGetComponent(out factionOwner);
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

        if (IsInventoryFull)
        {
            if (!TryStartDepositing())
            {
                StopHarvest();
            }
            return;
        }

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
        movementOwner?.StopMovement();
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

        int freeSpace = inventoryCapacity > 0 ? inventoryCapacity - inventoryCount : harvestAmount;
        if (freeSpace <= 0)
        {
            return 0;
        }

        int requested = Mathf.Min(harvestAmount, freeSpace);
        int harvested = target.Harvest(requested);
        lastHarvestTime = Time.time;

        if (harvested > 0)
        {
            AddToInventory(currentResourceType, harvested);
        }

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

    private bool IsInRange(Vector3 harvestPosition)
    {
        float distance = Vector3.Distance(transform.position, harvestPosition);
        return distance <= harvestRange;
    }

    private void SetCurrentTarget(IHarvestable target)
    {
        if (target == null)
            return;

        currentTarget = target;
        currentResourceType = target.GetResourceType();
    }

    private Faction GetFaction()
    {
        return factionOwner != null ? factionOwner.Faction : Faction.Neutral;
    }

    private void AddToInventory(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int freeSpace = inventoryCapacity > 0 ? inventoryCapacity - inventoryCount : amount;
        if (freeSpace <= 0)
        {
            return;
        }

        int added = inventoryCapacity > 0 ? Mathf.Min(amount, freeSpace) : amount;
        if (added <= 0)
        {
            return;
        }

        int current = inventory.GetValueOrDefault(type, 0);

        inventory[type] = current + added;
        inventoryCount += added;
    }

    private bool TryStartDepositing()
    {
        if (inventoryCount <= 0)
        {
            return false;
        }

        depositTarget = ResourceHolderConstructionRegistry.GetBestDropoff(GetFaction(), transform.position);
        if (depositTarget == null || depositTarget.GetGameObject() == null)
        {
            depositTarget = null;
            return false;
        }

        SetState(movingToDepositState);
        return true;
    }

    private bool IsInDepositRange(IResourceHolderConstruction construction)
    {
        if (construction == null || construction.GetGameObject() == null)
        {
            return false;
        }

        Vector3 pos = construction.GetNearestDepositPosition(transform.position);
        return Vector3.Distance(transform.position, pos) <= 0.1f;
    }

    private void DepositAll()
    {
        if (inventoryCount <= 0)
        {
            return;
        }

        Faction faction = GetFaction();
        foreach (var kvp in inventory)
        {
            PlayerResourceInventory.Add(faction, kvp.Key, kvp.Value);
        }

        inventory.Clear();
        inventoryCount = 0;

        if (onResourceInventoryChanged != null)
        {
            onResourceInventoryChanged.Raise();
        }
    }

    private void SetState(IUnitHarvesterControllerState nextState)
    {
        if (nextState == null || nextState == currentState)
            return;
        if (nextState.Name == "Idle")
        {
            if (currentState == null)
            {
                Debug.Log("Change to Idle from null");
            }
            else
            {
                Debug.Log("Change to Idle from " + currentState.Name);
            }
        }
        currentState?.Exit(this);
        currentState = nextState;
        currentState.Enter(this);
    }

    private void FaceTarget(IHarvestable target)
    {
        movementOwner.SetAutoRotate(false);
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
