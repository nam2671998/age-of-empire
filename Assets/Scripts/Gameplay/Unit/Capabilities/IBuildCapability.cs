using UnityEngine;

public interface IBuildCapability
{
    void SetBuildTarget(IBuildable target);
    void Build(IBuildable target);
    void StopBuilding();
}

