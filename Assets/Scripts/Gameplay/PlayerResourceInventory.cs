using System.Collections.Generic;
using UnityEngine;

public static class PlayerResourceInventory
{
    private static readonly Dictionary<Faction, Dictionary<ResourceType, int>> inventoryByFaction = new()
    {
        {Faction.Player1, new Dictionary<ResourceType, int>()
        {
            { ResourceType.Food, 100 }
        }}
    };

    [RuntimeInitializeOnLoadMethod]
    public static void Initialize()
    {
        inventoryByFaction.Clear();
        inventoryByFaction.TryAdd(Faction.Player1, new Dictionary<ResourceType, int>()
        {
            { ResourceType.Food, 100 }
        });
    }

    public static void Add(Faction faction, ResourceType resourceType, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (!inventoryByFaction.TryGetValue(faction, out var factionInventory))
        {
            factionInventory = new Dictionary<ResourceType, int>();
            inventoryByFaction[faction] = factionInventory;
        }

        int current = factionInventory.GetValueOrDefault(resourceType, 0);

        factionInventory[resourceType] = current + amount;
    }

    public static int GetAmount(Faction faction, ResourceType resourceType)
    {
        if (!inventoryByFaction.TryGetValue(faction, out var factionInventory))
        {
            return 0;
        }

        return factionInventory.GetValueOrDefault(resourceType, 0);
    }

    public static bool HasEnough(Faction faction, ResourceType resourceType, int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        return GetAmount(faction, resourceType) >= amount;
    }

    public static bool TrySpend(Faction faction, ResourceType resourceType, int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (!inventoryByFaction.TryGetValue(faction, out var factionInventory))
        {
            return false;
        }

        int current = factionInventory.GetValueOrDefault(resourceType, 0);
        if (current < amount)
        {
            return false;
        }

        int next = current - amount;
        if (next <= 0)
        {
            factionInventory.Remove(resourceType);
        }
        else
        {
            factionInventory[resourceType] = next;
        }

        return true;
    }
}

