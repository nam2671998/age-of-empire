using System;
using UnityEngine;

public class Farm : MonoBehaviour, IHarvestable
{
    [SerializeField] private Vector2Int size;
    [SerializeField] private ResourceType resourceType = ResourceType.Food;
    [SerializeField] private Transform harvestPositionTransform;
    [SerializeField] private Transform builtCenter;
    [SerializeField] private int maxResources = 100;
    
    private int currentResources;
    private Health health;
    private GridEntity gridEntity;
    
     void Start()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            currentResources = 0;
            health.HealthChanged += OnBuild;
            gameObject.layer = LayerMask.NameToLayer("Building");
        }
        gridEntity = GetComponent<GridEntity>();
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
        if (currentHealth >= maxHealth)
        {
            health.HealthChanged -= OnBuild;
            currentResources = maxResources;
            gameObject.layer = LayerMask.NameToLayer("Resource");
            gridEntity.OverrideSize(builtCenter.position, size);
        }
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
    
    public bool TryGetHarvestPosition(out Vector3 harvestPosition, IGridEntity harvester)
    {
        harvestPosition = harvestPositionTransform.position;
        Vector2Int harvestCell = GridManager.Instance.WorldToGrid(harvestPositionTransform.position);
        IGridEntity occupant = GridManager.Instance.GetCellReservation(harvestCell);
        bool isCellFree = GridManager.Instance.IsCellFree(harvestCell);
        return isCellFree || occupant == harvester;
    }

    private void OnDepleted()
    {
        Destroy(gameObject);
    }

    public bool CanHarvest()
    {
        return currentResources > 0;
    }
}
