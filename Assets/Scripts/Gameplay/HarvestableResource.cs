using UnityEngine;

/// <summary>
/// Example implementation of IHarvestable for resources that can be collected
/// </summary>
public class HarvestableResource : MonoBehaviour, IHarvestable
{
    [SerializeField] private ResourceType resourceType = ResourceType.None;
    [SerializeField] private int maxResources = 100;
    [SerializeField] private int harvestMultiplier = 1;
    [SerializeField] private int harvestPositionRadius = 1;
    [SerializeField] private int occupiedRadius = 1;

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
            
        int harvested = Mathf.Min(amount * harvestMultiplier, currentResources);
        currentResources -= harvested;
        UpdateCapacityState();
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
        Vector2Int center = GridManager.Instance.WorldToGrid(transform.position);
        for (int r = 1; r <= harvestPositionRadius; r++)
        {
            // Top & bottom edges
            for (int x = -r; x <= r; x++)
            {
                var cell = new Vector2Int(center.x + x, center.y + r);
                if (GridManager.Instance.IsCellFree(cell) || GridManager.Instance.GetCellReservation(cell) == harvester)
                {
                    harvestPosition = GridManager.Instance.GridToWorld(cell);
                    return true;
                }
                cell = new Vector2Int(center.x + x, center.y - r);
                if (GridManager.Instance.IsCellFree(cell) || GridManager.Instance.GetCellReservation(cell) == harvester)
                {
                    harvestPosition = GridManager.Instance.GridToWorld(cell);
                    return true;
                }
            }

            // Left & right edges (skip corners)
            for (int y = -r + 1; y <= r - 1; y++)
            {
                var cell = new Vector2Int(center.x + r, center.y + y);
                if (GridManager.Instance.IsCellFree(cell) || GridManager.Instance.GetCellReservation(cell) == harvester)
                {
                    harvestPosition = GridManager.Instance.GridToWorld(cell);
                    return true;
                }
                cell = new Vector2Int(center.x - r, center.y + y);
                if (GridManager.Instance.IsCellFree(cell) || GridManager.Instance.GetCellReservation(cell) == harvester)
                {
                    harvestPosition = GridManager.Instance.GridToWorld(cell);
                    return true;
                }
            }
        }

        harvestPosition = transform.position;
        return false;
    }

    private void OnDepleted()
    {
        this.Recycle();
    }

    private void UpdateCapacityState()
    {
        if (capacityStates == null || capacityStates.Length == 0)
        {
            return;
        }
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
    
    public bool CanHarvest()
    {
        return currentResources > 0;
    }
}

