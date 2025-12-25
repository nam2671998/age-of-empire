using UnityEngine;

public class MoveCommandResolver : ICommandResolver
{
    private readonly LayerMask groundLayer;

    public MoveCommandResolver(LayerMask groundLayer)
    {
        this.groundLayer = groundLayer;
    }

    public bool CanResolve(RaycastHit hit)
    {
        return IsInLayer(hit.collider.gameObject.layer, groundLayer);
    }

    public ICommand CreateCommand(RaycastHit hit)
    {
        return new MoveCommand(hit.point);
    }

    private static bool IsInLayer(int objectLayer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << objectLayer)) != 0;
    }
}

