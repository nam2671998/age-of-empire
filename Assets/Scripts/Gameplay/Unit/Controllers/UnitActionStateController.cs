using UnityEngine;

public class UnitActionStateController : MonoBehaviour
{
    private UnitActionState currentState = UnitActionState.Idle;
    private UnitMovementController movement;
    private UnitCombatController combat;
    private UnitHarvesterController harvester;
    private UnitBuilderController builder;
    
    public UnitActionState CurrentState => currentState;
    
    void Awake()
    {
        TryGetComponent(out movement);
        TryGetComponent(out combat);
        TryGetComponent(out harvester);
        TryGetComponent(out builder);
    }
    
    public void UpdateState()
    {
        UnitActionState newState = DetermineState();
        
        if (newState != currentState)
        {
            currentState = newState;
        }
    }
    
    private UnitActionState DetermineState()
    {
        if (builder != null && builder.IsBuilding)
        {
            return UnitActionState.Building;
        }
        
        if (combat != null && combat.IsAttacking)
        {
            return UnitActionState.Attacking;
        }
        
        if (harvester != null && harvester.IsHarvesting)
        {
            return UnitActionState.Harvesting;
        }
        
        if (movement != null && movement.IsMoving)
        {
            return UnitActionState.Moving;
        }
        
        return UnitActionState.Idle;
    }
    
    public void ResetToIdle()
    {
        currentState = UnitActionState.Idle;
    }
}

