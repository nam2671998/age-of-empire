using System.Collections.Generic;

public static class PlayerResourceInventory
{
    private static readonly Dictionary<Faction, Dictionary<ResourceType, int>> inventoryByFaction = new();

    public static void Initialize()
    {
        inventoryByFaction.Clear();
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
}

