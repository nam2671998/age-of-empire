using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UnitProduceConstruction : MonoBehaviour
{
    [SerializeField] private UnitProducingEventChannelSO onProduceUnit;
    [SerializeField] private ConstructionConfig constructionConfig;
    [SerializeField] private BoxCollider ownCollider;
    [SerializeField] private VoidEventChannelSO onResourceInventoryChanged;

    private IFactionOwner factionOwner;

    private void Awake()
    {
        factionOwner = GetComponentInParent<IFactionOwner>();
    }

    private void OnEnable()
    {
        if (onProduceUnit != null)
        {
            onProduceUnit.Register(ProduceUnit);
        }
    }
    
    private void OnDisable()
    {
        if (onProduceUnit != null)
        {
            onProduceUnit.Unregister(ProduceUnit);
        }
    }

    private async void ProduceUnit(UnitProducingData data)
    {
        if (constructionConfig == null || data.constructionId != constructionConfig.ConstructionId)
        {
            return;
        }

        Faction faction = factionOwner != null ? factionOwner.Faction : Faction.Player1;
        if (!TrySpendUnitCost(faction, data.unitId))
        {
            return;
        }

        if (onResourceInventoryChanged != null)
        {
            onResourceInventoryChanged.Raise();
        }

        GameObject unit = await Addressables.InstantiateAsync($"Units/{data.unitId}.prefab");
        if (unit == null || ownCollider == null)
        {
            return;
        }

        Bounds bounds = ownCollider.bounds;
        bounds.Expand(2);
        Vector3 spawnPos = bounds.min;
        spawnPos.y = 0;
        if (Random.Range(0, 2) == 0)
        {
            if (Random.Range(0, 2) == 0)
            {
                spawnPos.z = bounds.min.z;
                spawnPos.x = Random.Range(bounds.min.x, bounds.max.x);
            }
            else
            {
                spawnPos.z = bounds.max.z;
                spawnPos.x = Random.Range(bounds.min.x, bounds.max.x);
            }
        }
        else
        {
            if (Random.Range(0, 2) == 0)
            {
                spawnPos.x = bounds.min.x;
                spawnPos.z = Random.Range(bounds.min.z, bounds.max.z);
            }
            else
            {
                spawnPos.x = bounds.max.x;
                spawnPos.z = Random.Range(bounds.min.z, bounds.max.z);
            }
        }
        unit.transform.position = spawnPos;
    }

    private static bool TrySpendUnitCost(Faction faction, int unitId)
    {
        if (!ConfigManager.TryGetUnitCosts(unitId, out ResourceCost[] costs) || costs == null || costs.Length == 0)
        {
            return true;
        }

        foreach (var cost in costs)
        {
            if (!PlayerResourceInventory.HasEnough(faction, cost.ResourceType, cost.amount))
            {
                return false;
            }
        }

        foreach (var cost in costs)
        {
            if (!PlayerResourceInventory.TrySpend(faction, cost.ResourceType, cost.amount))
            {
                return false;
            }
        }

        return true;
    }
}
