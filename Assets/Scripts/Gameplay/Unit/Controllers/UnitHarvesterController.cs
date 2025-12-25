using UnityEngine;

public class UnitHarvesterController : MonoBehaviour, IHarvestCapability
{
    [Header("Harvest Settings")]
    [SerializeField] private int harvestAmount = 1;
    [SerializeField] private float harvestRange = 1;
    [SerializeField] private float harvestCooldown = 1f;

    [SerializeField] private UnitAnimatorController animator;
    
    private IHarvestable harvestTarget;
    private float lastHarvestTime = 0f;
    private bool isHarvesting = false;
    
    public IHarvestable HarvestTarget => harvestTarget;
    public bool IsHarvesting => isHarvesting;
    public int HarvestAmount => harvestAmount;
    public float HarvestRange => harvestRange;
    
    public void SetTarget(IHarvestable target)
    {
        harvestTarget = target;
    }
    
    public void SetHarvestTarget(IHarvestable target)
    {
        if (target != null && TryGetComponent(out SettlerUnit settler))
        {
            settler.StopOtherActions();
        }
        
        SetTarget(target);
        
        if (target != null && target.GetGameObject() != null && TryGetComponent(out IMovementCapability movement))
        {
            movement.MoveTo(target.GetHarvestPosition(), target.GetHarvestRadius());
        }
    }
    
    public bool CanHarvest()
    {
        return Time.time >= lastHarvestTime + harvestCooldown;
    }
    
    public int Harvest(IHarvestable target)
    {
        if (target == null || target.IsDepleted())
        {
            harvestTarget = null;
            isHarvesting = false;
            return 0;
        }
        
        if (!CanHarvest())
        {
            return 0;
        }
        
        FaceTarget(target);
        int harvested = 0;
        if (TryGetComponent(out Unit unit))
        {
            harvested = target.Harvest(harvestAmount, unit);
        }
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
        harvestTarget = null;
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

