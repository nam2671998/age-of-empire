using System.Collections.Generic;
using UnityEngine;

public class CommandExecutor : MonoBehaviour
{
    private Queue<ICommand> commandQueue = new Queue<ICommand>();
    private ICommand currentCommand = null;
    
    void Update()
    {
        if (currentCommand == null && commandQueue.Count > 0)
        {
            currentCommand = commandQueue.Dequeue();
            if (currentCommand != null)
            {
                currentCommand.Execute(this);
            }
        }
        
        if (currentCommand != null)
        {
            currentCommand.Update(this);
            
            if (currentCommand.IsCompleted(this))
            {
                currentCommand = null;
            }
        }
    }
    
    private void OnDisable()
    {
        // Free grid reservations when unit is destroyed
        if (GridManager.Instance != null)
        {
            GridManager.Instance.FreeUnitReservation(this);
        }
    }
    
    public void EnqueueCommand(ICommand command)
    {
        if (command == null)
        {
            Debug.LogWarning("CommandExecutor: Attempted to enqueue null command");
            return;
        }
        
        commandQueue.Enqueue(command);
    }
    
    public void SetCommand(ICommand command)
    {
        // Free any grid cell reservations when changing commands
        if (GridManager.Instance != null)
        {
            GridManager.Instance.FreeUnitReservation(this);
        }
        ClearCommands();
        EnqueueCommand(command);
    }
    
    public void ClearCommands()
    {
        if (currentCommand != null)
        {
            currentCommand.Cancel(this);
            currentCommand = null;
        }
        
        commandQueue.Clear();
    }
    
    public ICommand GetCurrentCommand()
    {
        return currentCommand;
    }
    
    public int GetQueueCount()
    {
        return commandQueue.Count;
    }
    
    public bool HasCommands()
    {
        return currentCommand != null || commandQueue.Count > 0;
    }
    
    public bool TryGetCapability<T>(out T capability) where T : class
    {
        return TryGetComponent(out capability);
    }
}

