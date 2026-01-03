using UnityEngine;

public class HarvestCommandResolver : ICommandResolver
{
    private readonly LayerMask resourceLayer;

    public HarvestCommandResolver(LayerMask resourceLayer)
    {
        this.resourceLayer = resourceLayer;
    }

    public bool CanResolve(RaycastHit hit)
    {
        if (!IsInLayer(hit.collider.gameObject.layer, resourceLayer))
            return false;

        return hit.collider.TryGetComponent(out IHarvestable harvestable) && harvestable.CanHarvest();
    }

    public ICommand CreateCommand(RaycastHit hit)
    {
        if (hit.collider != null && hit.collider.TryGetComponent(out IHarvestable harvestable))
            return new HarvestCommand(harvestable);

        return null;
    }

    private static bool IsInLayer(int objectLayer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << objectLayer)) != 0;
    }
}

