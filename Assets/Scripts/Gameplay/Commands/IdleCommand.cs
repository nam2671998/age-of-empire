using UnityEngine;

public class IdleCommand : BaseCommand
{
    public override void Execute(CommandExecutor executor)
    {
        if (executor.TryGetComponent(out UnitAnimatorController animator))
        {
            animator.TriggerIdle();
        }
    }

    public override bool IsCompleted(CommandExecutor executor)
    {
        return false;
    }

    public override string GetDescription()
    {
        return "Doing nothing";
    }
}
