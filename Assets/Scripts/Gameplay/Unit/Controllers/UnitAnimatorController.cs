using UnityEngine;

public class UnitAnimatorController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private bool useAnimator = true;
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private string isMovingParameter = "IsMoving";
    [SerializeField] private string isAttackingParameter = "IsAttacking";
    [SerializeField] private string isHarvestingParameter = "IsHarvesting";
    [SerializeField] private string isBuildingParameter = "IsBuilding";
    [SerializeField] private string attackTriggerParameter = "Attack";
    [SerializeField] private string harvestTriggerParameter = "Harvest";
    [SerializeField] private string buildTriggerParameter = "Build";
    [SerializeField] private string deathTriggerParameter = "Death";
    [SerializeField] private string idleTriggerParameter = "Idle";
    [SerializeField] private string moveTriggerParameter = "Move";
    
    [SerializeField] private Animator animator;
    private UnitActionState? previousState;
    
    public void UpdateSpeed(float speed)
    {
        SetAnimatorFloat(speedParameter, speed);
    }
    
    public void SetMoving(bool isMoving)
    {
        SetAnimatorBool(isMovingParameter, isMoving);
    }
    
    public void SetAttacking(bool isAttacking)
    {
        SetAnimatorBool(isAttackingParameter, isAttacking);
    }
    
    public void SetHarvesting(bool isHarvesting)
    {
        SetAnimatorBool(isHarvestingParameter, isHarvesting);
    }
    
    public void SetBuilding(bool isBuilding)
    {
        SetAnimatorBool(isBuildingParameter, isBuilding);
    }
    
    public void TriggerAttack()
    {
        TriggerAnimation(attackTriggerParameter);
    }
    
    public void TriggerHarvest()
    {
        TriggerAnimation(harvestTriggerParameter);
    }

    public void TriggerBuild()
    {
        TriggerAnimation(buildTriggerParameter);
    }
    
    public void TriggerDeath()
    {
        TriggerAnimation(deathTriggerParameter);
    }
    
    public void TriggerIdle()
    {
        TriggerAnimation(idleTriggerParameter);
    }

    public void TriggerMove()
    {
        TriggerAnimation(moveTriggerParameter);
    }
    
    public void UpdateState(UnitActionState state, float speed)
    {
        if (!useAnimator || animator == null)
            return;
        
        UpdateSpeed(speed);
        SetMoving(state == UnitActionState.Moving);
        SetAttacking(state == UnitActionState.Attacking);
        SetHarvesting(state == UnitActionState.Harvesting);
        SetBuilding(state == UnitActionState.Building);

        if (!previousState.HasValue || previousState.Value != state)
        {
            if (state == UnitActionState.Moving)
                TriggerMove();
            else if (state == UnitActionState.Idle)
                TriggerIdle();

            previousState = state;
        }
    }
    
    private void TriggerAnimation(string triggerName)
    {
        if (!useAnimator || animator == null || string.IsNullOrEmpty(triggerName))
            return;
        animator.SetTrigger(triggerName);
    }
    
    private void SetAnimatorBool(string parameterName, bool value)
    {
        if (!useAnimator || animator == null || string.IsNullOrEmpty(parameterName))
            return;
        animator.SetBool(parameterName, value);
    }
    
    private void SetAnimatorFloat(string parameterName, float value)
    {
        if (!useAnimator || animator == null || string.IsNullOrEmpty(parameterName))
            return;
        animator.SetFloat(parameterName, value);
    }
}

