using System;
using UnityEngine;

public class Farm : MonoBehaviour, IHarvestable
{
    [SerializeField] private ResourceType resourceType = ResourceType.Food;
    [SerializeField] private Transform harvestPositionTransform;
    [SerializeField] private int maxResources = 100;
    
    private int currentResources;
    private Health health;
    
     void Start()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            currentResources = 0;
            health.HealthChanged += OnBuild;
            gameObject.layer = LayerMask.NameToLayer("Building");
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.HealthChanged -= OnBuild;
        }
    }
    
    private void OnBuild(int currentHealth, int maxHealth)
    {
        currentResources = maxResources;
        gameObject.layer = LayerMask.NameToLayer("Resource");
    }
    
    public int Harvest(int amount)
    {
        if (health.CurrentHealth < health.MaxHealth)
            return 0;
        
        if (IsDepleted())
            return 0;
            
        int harvested = Mathf.Min(amount, currentResources);
        currentResources -= harvested;
        if (currentResources <= 0)
        {
            OnDepleted();
        }

        return harvested;
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
        try
        {
            return gameObject;
        }
        catch
        {
            return null;
        }
    }
    
    public Vector3 GetPosition()
    {
        return transform.position;
    }
    
    public bool TryGetHarvestPosition(out Vector3 harvestPosition)
    {
        harvestPosition = harvestPositionTransform.position;
        Vector2Int harvestCell = GridManager.Instance.WorldToGrid(harvestPositionTransform.position);
        return GridManager.Instance.IsCellFree(harvestCell);
    }

    private void OnDepleted()
    {
        Destroy(gameObject);
    }
}
