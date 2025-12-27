using UnityEngine;

public class UnitHarvesterController : MonoBehaviour, IHarvestCapability
{
    [Header("Harvest Settings")]
    [SerializeField] private int harvestAmount = 1;
    [SerializeField] private float harvestRange = 1;
    [SerializeField] private float harvestCooldown = 1f;

    [SerializeField] private UnitAnimatorController animator;
    
    private float lastHarvestTime = 0f;
    private bool isHarvesting = false;
    public bool IsHarvesting => isHarvesting;
    
    public void SetHarvestTarget(IHarvestable target)
    {
        if (target != null && TryGetComponent(out IStopAction s))
        {
            s.StopOtherActions();
        }
        
        if (target != null && target.GetGameObject() != null && TryGetComponent(out IMovementCapability movement))
        {
            movement.MoveTo(target.GetHarvestPosition(), target.GetHarvestRadius());
        }
    }
    
    public bool CanHarvest()
    {
        return Time.time >= lastHarvestTime + harvestCooldown;
    }

    public void StopHarvest()
    {
        isHarvesting = false;
    }

    public int Harvest(IHarvestable target)
    {
        if (target == null || target.IsDepleted())
        {
            isHarvesting = false;
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
    
    public void StopHarvesting()
    {
        isHarvesting = false;
    }
    
    public bool IsInRange(IHarvestable target)
    {
        if (target == null)
            return false;
        
        float distance = Vector3.Distance(transform.position, target.GetHarvestPosition());
        return distance <= harvestRange;
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

