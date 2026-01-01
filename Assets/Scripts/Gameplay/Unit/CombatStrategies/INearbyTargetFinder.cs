using UnityEngine;

public interface INearbyTargetFinder
{
    bool TryFindNearbyTarget(Faction attackerFaction, Vector3 origin, float searchRadius, out IDamageable target);
}
