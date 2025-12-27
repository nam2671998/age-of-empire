using UnityEngine;

public interface IBuildable
{
    bool Build(float percentage);
    Vector3 GetNearestBuildPosition(Vector3 from);
    GameObject GetGameObject();
    bool IsComplete();
}
