using UnityEngine;

public interface ICommandResolver
{
    bool CanResolve(RaycastHit hit);
    ICommand CreateCommand(RaycastHit hit);
}

