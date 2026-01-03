using System;
using System.Collections.Generic;
using UnityEngine;

public static class ConfigManager
{
    private static bool initialized;
    private static readonly Dictionary<int, ResourceCost[]> constructionCostsById = new();
    private static readonly Dictionary<int, ResourceCost[]> unitCostsById = new();
    public static List<BuildOption> BuildableConstructions { get; } = new()
    {
        new BuildOption(1001001, "Barrack"),
        new BuildOption(1001002, "Archery"),
        new BuildOption(1001003, "Granary"),
        new BuildOption(1001004, "Farm")
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        initialized = true;
        constructionCostsById.Clear();
        unitCostsById.Clear();

        LoadConstructionCosts();
        LoadUnitCosts();
    }

    private static void LoadConstructionCosts()
    {
        TextAsset configAsset = Resources.Load<TextAsset>("ConstructionCosts");
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

            constructionCostsById[entry.id] = entry.costs;
        }
    }

    public static bool TryGetConstructionCosts(int constructionId, out ResourceCost[] costs)
    {
        return constructionCostsById.TryGetValue(constructionId, out costs);
    }

    public static bool TryGetUnitCosts(int unitId, out ResourceCost[] costs)
    {
        return unitCostsById.TryGetValue(unitId, out costs);
    }

    private static void LoadUnitCosts()
    {
        TextAsset configAsset = Resources.Load<TextAsset>("UnitCosts");
        if (configAsset == null || string.IsNullOrWhiteSpace(configAsset.text))
        {
            return;
        }

        UnitCostsConfig config = JsonUtility.FromJson<UnitCostsConfig>(configAsset.text);
        if (config == null || config.units == null)
        {
            return;
        }

        for (int i = 0; i < config.units.Length; i++)
        {
            UnitCostsEntry entry = config.units[i];
            if (entry == null || entry.id == 0 || entry.costs == null)
            {
                continue;
            }

            unitCostsById[entry.id] = entry.costs;
        }
    }

    [Serializable]
    private class ConstructionCostsConfig
    {
        public ConstructionCostsEntry[] constructions;
    }

    [Serializable]
    private class ConstructionCostsEntry
    {
        public int id;
        public ResourceCost[] costs;
    }

    [Serializable]
    private class UnitCostsConfig
    {
        public UnitCostsEntry[] units;
    }

    [Serializable]
    private class UnitCostsEntry
    {
        public int id;
        public ResourceCost[] costs;
    }
}

public readonly struct BuildOption
{
    public readonly int id;
    public readonly string displayName;

    public BuildOption(int id, string displayName)
    {
        this.id = id;
        this.displayName = displayName;
    }
}

[Serializable]
public struct ResourceCost
{
    public int type;
    public int amount;
    public ResourceType ResourceType => (ResourceType)type;
}

