using UnityEngine;

public interface IResourceHolderConstruction : IFactionOwner
{
    int Priority { get; }
    GameObject GetGameObject();
    Vector3 GetDepositPosition();
    float GetDepositRadius();
}

