using System;
using System.Collections.Generic;
using UnityEngine;

public static class ConfigManager
{
    public readonly struct ResourceCost
    {
        public readonly ResourceType ResourceType;
        public readonly int Amount;

        public ResourceCost(ResourceType resourceType, int amount)
        {
            ResourceType = resourceType;
            Amount = amount;
        }
    }

    private const string constructionCostsResourcePath = "ConstructionCosts";

    private static bool initialized;
    private static readonly Dictionary<int, List<ResourceCost>> constructionCostsById = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        initialized = true;
        constructionCostsById.Clear();

        TextAsset configAsset = Resources.Load<TextAsset>(constructionCostsResourcePath);
        if (configAsset == null || string.IsNullOrWhiteSpace(configAsset.text))
        {
            return;
        }

        ConstructionCostsConfig config = JsonUtility.FromJson<ConstructionCostsConfig>(configAsset.text);
        if (config == null || config.constructions == null)
        {
            return;
        }

        for (int i = 0; i < config.constructions.Length; i++)
        {
            ConstructionCostsEntry entry = config.constructions[i];
            if (entry == null || entry.id == 0 || entry.costs == null)
            {
                continue;
            }

            var list = new List<ResourceCost>(entry.costs.Length);
            for (int j = 0; j < entry.costs.Length; j++)
            {
                ResourceCostEntry cost = entry.costs[j];
                if (cost == null || cost.amount <= 0 || string.IsNullOrWhiteSpace(cost.type))
                {
                    continue;
                }

                if (!Enum.TryParse(cost.type, ignoreCase: true, out ResourceType parsedType))
                {
                    continue;
                }

                list.Add(new ResourceCost(parsedType, cost.amount));
            }

            constructionCostsById[entry.id] = list;
        }
    }

    public static bool TryGetConstructionCosts(int constructionId, out List<ResourceCost> costs)
    {
        EnsureInitialized();
        return constructionCostsById.TryGetValue(constructionId, out costs);
    }

    private static void EnsureInitialized()
    {
        if (!initialized)
        {
            Initialize();
        }
    }

    [Serializable]
    private sealed class ConstructionCostsConfig
    {
        public ConstructionCostsEntry[] constructions;
    }

    [Serializable]
    private sealed class ConstructionCostsEntry
    {
        public int id;
        public ResourceCostEntry[] costs;
    }

    [Serializable]
    private sealed class ResourceCostEntry
    {
        public string type;
        public int amount;
    }
}

