using UnityEngine;

public interface IBuildCapability
{
    void SetBuildTarget(Vector3 position, GameObject buildingPrefab);
    void StartBuilding();
    void StopBuilding();
    bool IsBuilding();
    float BuildRange { get; }
    bool IsInRange();
}

