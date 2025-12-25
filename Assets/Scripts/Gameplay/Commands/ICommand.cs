using UnityEngine;

public interface ICommand
{
    void Execute(CommandExecutor executor);
    bool IsCompleted(CommandExecutor executor);
    void Update(CommandExecutor executor);
    void Cancel(CommandExecutor executor);
    string GetDescription();
}

