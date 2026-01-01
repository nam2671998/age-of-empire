using UnityEngine;

public interface IBuildable
{
    bool Build(int progress);
    Vector3 GetNearestBuildPosition(Vector3 from);
    bool TryReserveBuildPosition(CommandExecutor executor, out Vector3 position);
    void ReleaseBuildPosition(IMovementCapability executor);
    GameObject GetGameObject();
    bool IsComplete();
    void Preview();
    void Place();
}
