using UnityEngine;

public class UnitHarvesterController : MonoBehaviour, IHarvestCapability
{
    [Header("Harvest Settings")]
    [SerializeField] private int harvestAmount = 1;
    [SerializeField] private float harvestRange = 1;
    [SerializeField] private float harvestCooldown = 1f;
    [SerializeField] private float findNextRadius = 10f;

    [SerializeField] private LayerMask harvestableResourceMask;

    [SerializeField] private UnitAnimatorController animator;
    
    private float lastHarvestTime = 0f;
    private bool isHarvesting = false;
    private IHarvestable currentTarget;
    private ResourceType currentResourceType;
    private Collider[] overlapResults = new Collider[32];
    public bool IsHarvesting => isHarvesting;

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
        isHarvesting = true;
        lastHarvestTime = Time.time - harvestCooldown;

        if (TryGetComponent(out IMovementCapability movement))
        {
            movement.MoveTo(target.GetHarvestPosition(), target.GetHarvestRadius());
        }
    }

    public void TickHarvest()
    {
        if (!isHarvesting)
            return;

        if (currentTarget == null || currentTarget.GetGameObject() == null)
        {
            StopHarvest();
            return;
        }

        if (currentTarget.IsDepleted())
        {
            OnHarvestCompleted();
            return;
        }

        if (!TryGetComponent(out IMovementCapability movement))
        {
            StopHarvest();
            return;
        }

        if (!IsInRange(currentTarget))
        {
            movement.MoveTo(currentTarget.GetHarvestPosition(), currentTarget.GetHarvestRadius());
            return;
        }

        movement.StopMovement();
        if (!CanHarvest())
            return;

        Harvest(currentTarget);
        if (currentTarget == null || currentTarget.GetGameObject() == null)
        {
            StopHarvest();
            return;
        }

        if (currentTarget.IsDepleted())
        {
            OnHarvestCompleted();
        }
    }
    
    public void SetHarvestTarget(IHarvestable target)
    {
        StartHarvest(target);
    }
    
    public bool CanHarvest()
    {
        return Time.time >= lastHarvestTime + harvestCooldown;
    }

    public void StopHarvest()
    {
        isHarvesting = false;
        currentTarget = null;
        animator.TriggerIdle();
    }

    public int Harvest(IHarvestable target)
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
        int harvested = 0;
        harvested = target.Harvest(harvestAmount);
        lastHarvestTime = Time.time;
        isHarvesting = true;

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
    
    public bool IsInRange(IHarvestable target)
    {
        if (target == null)
            return false;
        
        float distance = Vector3.Distance(transform.position, target.GetHarvestPosition());
        return distance <= harvestRange;
    }

    private void OnHarvestCompleted()
    {
        var next = FindNextHarvestable();
        if (next == null)
        {
            if (TryGetComponent(out IMovementCapability movement))
            {
                movement.StopMovement();
            }
            StopHarvest();
            return;
        }

        currentTarget = next;
        currentResourceType = next.GetResourceType();
        isHarvesting = true;

        if (TryGetComponent(out IMovementCapability nextMovement))
        {
            nextMovement.MoveTo(next.GetHarvestPosition(), next.GetHarvestRadius());
        }
    }

    private IHarvestable FindNextHarvestable()
    {
        if (currentTarget == null || currentTarget.GetGameObject() == null)
            return null;

        var currentGo = currentTarget.GetGameObject();
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, findNextRadius, overlapResults, harvestableResourceMask);

        IHarvestable best = null;
        float bestSqrDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            var col = overlapResults[i];
            if (col == null)
                continue;

            if (!col.TryGetComponent<IHarvestable>(out var candidate))
                continue;

            if (candidate == null)
                continue;

            var candidateGo = candidate.GetGameObject();
            if (candidateGo == null || candidateGo == currentGo || !candidateGo.activeInHierarchy)
                continue;

            if (candidate.IsDepleted())
                continue;

            if (candidate.GetResourceType() != currentResourceType)
                continue;

            Vector3 candidatePos = candidate.GetHarvestPosition();
            float sqr = (candidatePos - transform.position).sqrMagnitude;
            if (sqr < bestSqrDistance)
            {
                bestSqrDistance = sqr;
                best = candidate;
            }
        }

        return best;
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
}

