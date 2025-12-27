using UnityEngine;

public interface IBuildCapability
{
    void SetBuildTarget(IBuildable target);
    void Build(IBuildable target);
    bool CanBuild();
    void StopBuilding();
    bool IsInRange(IBuildable target);
    GameObject GetGameObject();
}

