using UnityEngine;

[RequireComponent(typeof(SelectableObject))]
[RequireComponent(typeof(UnitMovementController))]
[RequireComponent(typeof(UnitActionStateController))]
[RequireComponent(typeof(CommandExecutor))]
public abstract class Unit : MonoBehaviour, IFactionOwner
{
    [Header("Faction")]
    [SerializeField] protected Faction faction = Faction.Player1;
    
    protected CommandExecutor commandExecutor;
    protected UnitMovementController movement;
    protected UnitActionStateController stateManager;
    protected UnitAnimatorController animator;
    
    public Faction Faction => faction;
    
    void Awake()
    {
        InitializeComponents();
        InitializeUnit();
    }
    
    protected virtual void InitializeComponents()
    {
        TryGetComponent(out commandExecutor);
        TryGetComponent(out movement);
        TryGetComponent(out stateManager);
        TryGetComponent(out animator);
    }
    
    protected abstract void InitializeUnit();
    
    void Update()
    {
        if (movement != null && movement.IsMoving)
        {
            movement.UpdateMovement();
        }
        
        if (stateManager != null)
        {
            stateManager.UpdateState();
        }
        
        if (animator != null)
        {
            float speed = movement != null ? movement.GetCurrentSpeed() : 0f;
            animator.UpdateState(stateManager.CurrentState, speed);
        }
        
        UpdateUnit();
    }
    
    protected virtual void UpdateUnit() { }
    
    #region Movement
    
    public void MoveTo(Vector3 targetPosition, float stoppingDistance = 0.5f)
    {
        if (movement == null)
            return;
        
        movement.MoveTo(targetPosition, stoppingDistance);
    }
    
    public void StopMovement()
    {
        if (movement != null)
        {
            movement.Stop();
        }
    }
    
    public bool IsMoving()
    {
        return movement != null && movement.IsMoving;
    }
    
    public float GetMoveSpeed()
    {
        return movement != null ? movement.MoveSpeed : 0f;
    }
    
    #endregion
    
    #region Command Queue (Delegates to CommandExecutor)
    
    public void AddCommand(ICommand command)
    {
        if (commandExecutor != null)
        {
            commandExecutor.EnqueueCommand(command);
        }
    }
    
    public void SetCommand(ICommand command)
    {
        if (commandExecutor != null)
        {
            commandExecutor.SetCommand(command);
        }
    }
    
    public void ClearCommands()
    {
        if (commandExecutor != null)
        {
            commandExecutor.ClearCommands();
        }
        
        StopMovement();
        
        if (stateManager != null)
        {
            stateManager.ResetToIdle();
        }
    }
    
    public ICommand GetCurrentCommand()
    {
        return commandExecutor != null ? commandExecutor.GetCurrentCommand() : null;
    }
    
    public int GetQueueCount()
    {
        return commandExecutor != null ? commandExecutor.GetQueueCount() : 0;
    }
    
    #endregion
    
    #region Animation
    
    public void PlayDeathAnimation()
    {
        if (animator != null)
        {
            animator.TriggerDeath();
        }
    }
    
    public UnitActionState GetCurrentActionState()
    {
        return stateManager != null ? stateManager.CurrentState : UnitActionState.Idle;
    }
    
    #endregion
    
}

public enum UnitActionState
{
    Idle,
    Moving,
    Attacking,
    Harvesting,
    Building,
    Dead
}


