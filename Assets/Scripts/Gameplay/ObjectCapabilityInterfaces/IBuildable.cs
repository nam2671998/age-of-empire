using UnityEngine;

public interface IBuildable
{
    bool Build(float percentage);
    Vector3 GetNearestBuildPosition(Vector3 from);
    bool TryReserveBuildPosition(CommandExecutor executor, out Vector3 position);
    void ReleaseBuildPosition(CommandExecutor executor);
    GameObject GetGameObject();
    bool IsComplete();
}
