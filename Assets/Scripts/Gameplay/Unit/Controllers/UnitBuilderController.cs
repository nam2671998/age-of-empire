using UnityEngine;
using UnityEngine.Serialization;

public class UnitBuilderController : MonoBehaviour, IBuildCapability
{
    [SerializeField] private float buildingPercentage = 0.1f;
    [SerializeField] private float buildCooldown = 1f;
    
    [SerializeField] private UnitAnimatorController animator;
    
    private float lastBuildTime = 0f;
    private bool isBuilding = false;
    public bool IsBuilding => isBuilding;
    
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

    public void SetBuildTarget(IBuildable target)
    {
        if (target != null && TryGetComponent(out IStopAction s))
        {
            s.StopOtherActions();
        }
        
        if (target != null && target.GetGameObject() != null && TryGetComponent(out IMovementCapability movement))
        {
            movement.MoveTo(target.GetNearestBuildPosition(movement.transform.position), 0);
        }
    }

    public void Build(IBuildable target)
    {
        if (target == null || target.IsComplete())
        {
            isBuilding = false;
            return;
        }
        
        if (!CanBuild())
        {
            animator.TriggerIdle();
            return;
        }
        
        FaceTarget(target);
        bool progressed = target.Build(buildingPercentage);
        lastBuildTime = Time.time;
        isBuilding = true;

        if (animator != null)
        {
            if (progressed && !target.IsComplete())
            {
                animator.TriggerBuild();
            }
            else
            {
                animator.TriggerIdle();
            }
        }
    }

    public bool CanBuild()
    {
        return Time.time >= lastBuildTime + buildCooldown;
    }

    public void StopBuilding()
    {
        isBuilding = false;
    }

    public bool IsInRange(IBuildable target)
    {
        if (target == null)
            return false;
        Vector3 position = transform.position;
        Vector3 nearestBuildPosition = target.GetNearestBuildPosition(position);
        position.y = nearestBuildPosition.y;
        return Vector3.Distance(position, nearestBuildPosition) < 0.01f;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    private void FaceTarget(IBuildable target)
    {
        Vector3 direction = target.GetGameObject().transform.position - target.GetNearestBuildPosition(transform.position);
        direction.y = 0f;
        
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f * Time.deltaTime);
        }
    }
}

