using UnityEngine;

public interface IResourceHolderConstruction : IFactionOwner
{
    int Priority { get; }
    GameObject GetGameObject();
    Vector3 GetNearestDepositPosition(Vector3 from);
}

