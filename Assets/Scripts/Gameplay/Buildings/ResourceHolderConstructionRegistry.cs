using System.Collections.Generic;
using UnityEngine;

public static class ResourceHolderConstructionRegistry
{
    private static readonly Dictionary<Faction, HashSet<IResourceHolderConstruction>> holdersByFaction = new();

    [RuntimeInitializeOnLoadMethod]
    public static void Initialize()
    {
        holdersByFaction.Clear();
    }

    public static void Register(IResourceHolderConstruction construction)
    {
        if (construction == null || construction.GetGameObject() == null)
        {
            return;
        }

        if (!holdersByFaction.TryGetValue(construction.Faction, out var list))
        {
            list = new HashSet<IResourceHolderConstruction>();
            holdersByFaction[construction.Faction] = list;
        }

        list.Add(construction);
    }

    public static void Unregister(IResourceHolderConstruction construction)
    {
        if (construction == null)
        {
            return;
        }

        Faction faction = construction.Faction;
        if (holdersByFaction.TryGetValue(faction, out var list))
        {
            list.Remove(construction);
        }
    }

    public static IResourceHolderConstruction GetBestDropoff(Faction faction, Vector3 fromPosition)
    {
        if (!holdersByFaction.TryGetValue(faction, out var list) || list.Count == 0)
        {
            return null;
        }

        int lowestPriority = int.MaxValue;
        IResourceHolderConstruction result = null;
        float shortestDistance = float.PositiveInfinity;
        bool hasNull = false;

        foreach (var construction in list)
        {
            if (construction == null || construction.GetGameObject() == null)
            {
                hasNull = true;
                continue;
            }

            int priority = construction.Priority;
            Vector3 pos = construction.GetNearestDepositPosition(fromPosition);
            float sqr = (pos - fromPosition).sqrMagnitude;

            if (priority < lowestPriority)
            {
                lowestPriority = priority;
                result = construction;
                shortestDistance = sqr;
                continue;
            }

            if (priority == lowestPriority && sqr < shortestDistance)
            {
                result = construction;
                shortestDistance = sqr;
            }
        }

        if (hasNull)
        {
            list.RemoveWhere(static e => e == null);
        }

        return result;
    }
}

