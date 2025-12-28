using UnityEngine;

public class AttackCommandResolver : ICommandResolver
{
    private readonly LayerMask unitLayer;

    public AttackCommandResolver(LayerMask unitLayer)
    {
        this.unitLayer = unitLayer;
    }

    public bool CanResolve(RaycastHit hit)
    {
        if (!IsInLayer(hit.collider.gameObject.layer, unitLayer))
            return false;

        return hit.collider.TryGetComponent<IDamageable>(out _);
    }

    public ICommand CreateCommand(RaycastHit hit)
    {
        if (hit.collider != null && hit.collider.TryGetComponent<IDamageable>(out var attackable))
            return new AttackCommand(attackable);

        return null;
    }

    private static bool IsInLayer(int objectLayer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << objectLayer)) != 0;
    }
}

