using UnityEngine;

/// <summary>
/// Example implementation of IHarvestable for resources that can be collected
/// </summary>
public class HarvestableResource : MonoBehaviour, IHarvestable
{
    [SerializeField] private ResourceType resourceType = ResourceType.Generic;
    [SerializeField] private int maxResources = 100;
    
    [SerializeField] private Transform harvestPositionTransform;
    [SerializeField] private float harvestPositionRadius = 1;

    [SerializeField] private GameObject[] capacityStates;
    
    private int currentResources;
    
    void Start()
    {
        currentResources = maxResources;
        UpdateCapacityState();
    }
    
    public int Harvest(int amount)
    {
        if (IsDepleted())
            return 0;
            
        int harvested = Mathf.Min(amount, currentResources);
        currentResources -= harvested;
        UpdateCapacityState();
        
        Debug.Log($"{gameObject.name} is harvested and gain {harvested} {resourceType}. Remaining: {currentResources}/{maxResources}");
        
        // Handle depletion
        if (currentResources <= 0)
        {
            OnDepleted();
        }

        return harvested;
    }
    
    public float GetRemainingResources()
    {
        return currentResources;
    }
    
    public float GetMaxResources()
    {
        return maxResources;
    }
    
    public ResourceType GetResourceType()
    {
        return resourceType;
    }
    
    public bool IsDepleted()
    {
        return currentResources <= 0;
    }
    
    public GameObject GetGameObject()
    {
        return gameObject;
    }
    
    public Vector3 GetPosition()
    {
        return transform.position;
    }
    
    public Vector3 GetHarvestPosition()
    {
        if (harvestPositionTransform != null)
        {
            return harvestPositionTransform.position;
        }
        
        return transform.position;
    }

    public float GetHarvestRadius()
    {
        return harvestPositionRadius;
    }

    private void OnDepleted()
    {
        Debug.Log($"{gameObject.name} has been depleted");
        
        // gameObject.SetActive(false);
    }

    private void UpdateCapacityState()
    {
        if (currentResources == 0)
        {
            SetCapacityState(0);
            return;
        }
        float amountLeft = currentResources * 1f / maxResources;
        if (amountLeft < 0.5f)
        {
            SetCapacityState(1);
        }
        else
        {
            SetCapacityState(2);
        }
    }
    private void SetCapacityState(int state)
    {
        for (int i = 0; i < capacityStates.Length; i++)
        {
            capacityStates[i].SetActive(i == state);
        }
    }
}

