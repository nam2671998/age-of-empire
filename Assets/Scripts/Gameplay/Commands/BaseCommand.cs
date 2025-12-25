using UnityEngine;

public abstract class BaseCommand : ICommand
{
    protected bool isExecuting = false;
    protected bool isCompleted = false;
    protected bool isCancelled = false;
    
    public abstract void Execute(CommandExecutor executor);
    
    public virtual bool IsCompleted(CommandExecutor executor)
    {
        return isCompleted || isCancelled;
    }
    
    public virtual void Update(CommandExecutor executor)
    {
        if (isCancelled || isCompleted)
            return;
            
        if (!isExecuting)
        {
            isExecuting = true;
            OnStart(executor);
        }
        
        OnUpdate(executor);
    }
    
    public virtual void Cancel(CommandExecutor executor)
    {
        if (isCancelled || isCompleted)
            return;
        
        isCancelled = true;
        OnCancel(executor);
    }
    
    public abstract string GetDescription();
    
    protected virtual void OnStart(CommandExecutor executor) { }
    protected virtual void OnUpdate(CommandExecutor executor) { }
    protected virtual void OnCancel(CommandExecutor executor) { }
    
    protected void Complete()
    {
        isCompleted = true;
    }
}

