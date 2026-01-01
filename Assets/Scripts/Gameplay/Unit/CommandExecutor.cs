using System.Collections.Generic;
using UnityEngine;

public class CommandExecutor : MonoBehaviour
{
    private Queue<ICommand> commandQueue = new Queue<ICommand>();
    private ICommand currentCommand = null;
    private ICommand idleCommand = new IdleCommand();

    private bool IsIdle => currentCommand == null || currentCommand == idleCommand;
    
    void Update()
    {
        if (IsIdle)
        {
            if (commandQueue.Count > 0)
            {
                currentCommand = commandQueue.Dequeue();
                if (!IsIdle)
                {
                    currentCommand.Execute(this);
                }
            }
            else
            {
                currentCommand = idleCommand;
            }
        }
        
        if (!IsIdle)
        {
            currentCommand.Update(this);
            
            if (currentCommand.IsCompleted(this))
            {
                currentCommand = null;
            }
        }
    }
    
    public void EnqueueCommand(ICommand command)
    {
        if (command == null)
        {
            Debug.LogError("CommandExecutor: Attempted to enqueue null command");
            return;
        }

        if (currentCommand == idleCommand)
        {
            currentCommand = null;
        }
        
        commandQueue.Enqueue(command);
    }
    
    public void SetCommand(ICommand command)
    {
        ClearCommands();
        EnqueueCommand(command);
    }
    
    public void ClearCommands()
    {
        if (currentCommand == idleCommand)
        {
            return;
        }
        if (currentCommand != null)
        {
            currentCommand.Cancel(this);
            currentCommand = null;
        }
        
        commandQueue.Clear();
    }
    
    public bool TryGetCapability<T>(out T capability) where T : class
    {
        return TryGetComponent(out capability);
    }
}

