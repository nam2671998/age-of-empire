using UnityEngine;

[RequireComponent(typeof(CommandExecutor))]
[RequireComponent(typeof(UnitSelectable))]
[RequireComponent(typeof(UnitMovementController))]
[RequireComponent(typeof(UnitActionStateController))]
public abstract class Unit : MonoBehaviour
{
    protected UnitMovementController movement;
    protected UnitActionStateController stateManager;
    protected UnitAnimatorController animator;
    
    void Awake()
    {
        InitializeComponents();
        InitializeUnit();
    }
    
    protected virtual void InitializeComponents()
    {
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


