using UnityEngine;

public class BuildCommandResolver : ICommandResolver
{
    private readonly LayerMask buildingLayer;

    public BuildCommandResolver(LayerMask buildingLayer)
    {
        this.buildingLayer = buildingLayer;
    }

    public bool CanResolve(RaycastHit hit)
    {
        if (!IsInLayer(hit.collider.gameObject.layer, buildingLayer))
            return false;

        return hit.collider.GetComponentInParent<IBuildable>() != null;
    }

    public ICommand CreateCommand(RaycastHit hit)
    {
        IBuildable buildableConstruction = hit.collider != null ? hit.collider.GetComponentInParent<IBuildable>() : null;
        if (buildableConstruction != null)
            return new BuildCommand(buildableConstruction);

        return null;
    }

    private static bool IsInLayer(int objectLayer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << objectLayer)) != 0;
    }
}

