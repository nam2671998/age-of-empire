using UnityEngine;

public class BuildCommand : BaseCommand
{
    private GameObject buildingPrefab;
    private Vector3 buildPosition;
    private Quaternion buildRotation;
    private float buildRange = 3f;
    private float buildTime = 5f;
    private float buildProgress = 0f;
    private bool isBuilding = false;
    
    public BuildCommand(GameObject buildingPrefab, Vector3 position, Quaternion rotation, float buildTime = 5f, float buildRange = 3f)
    {
        this.buildingPrefab = buildingPrefab;
        this.buildPosition = position;
        this.buildRotation = rotation;
        this.buildTime = buildTime;
        this.buildRange = buildRange;
    }
    
    public override void Execute(CommandExecutor executor)
    {
        if (executor == null)
        {
            Debug.LogError("BuildCommand: Executor is null");
            return;
        }
        
        if (buildingPrefab == null)
        {
            Debug.LogError("BuildCommand: Building prefab is null");
            Complete();
            return;
        }
        
        if (executor.TryGetCapability(out IBuildCapability buildUnit))
        {
            buildUnit.SetBuildTarget(buildPosition, buildingPrefab);
        }
        else
        {
            Debug.LogWarning($"BuildCommand: IBuildUnit capability not found");
            Complete();
        }
    }
    
    protected override void OnUpdate(CommandExecutor executor)
    {
        if (executor == null)
        {
            Complete();
            return;
        }
        
        if (!executor.TryGetCapability(out IBuildCapability buildUnit))
        {
            Complete();
            return;
        }
        
        if (!executor.TryGetCapability(out IMovementCapability movement))
        {
            Complete();
            return;
        }
        
        if (!buildUnit.IsInRange())
        {
            movement.MoveTo(buildPosition, buildUnit.BuildRange * 0.8f);
            isBuilding = false;
            return;
        }
        
        if (!isBuilding)
        {
            isBuilding = true;
            buildProgress = 0f;
            buildUnit.StartBuilding();
        }
        
        buildProgress += Time.deltaTime;
        
        if (buildProgress >= buildTime)
        {
            GameObject building = Object.Instantiate(buildingPrefab, buildPosition, buildRotation);
            Debug.Log($"Building {buildingPrefab.name} completed at {buildPosition}", building);
            
            Complete();
        }
    }
    
    protected override void OnCancel(CommandExecutor executor)
    {
        if (executor != null && executor.TryGetCapability(out IBuildCapability buildUnit))
        {
            buildUnit.StopBuilding();
        }
        isBuilding = false;
        buildProgress = 0f;
    }
    
    public override string GetDescription()
    {
        if (buildingPrefab == null)
            return "Build (invalid prefab)";
            
        float progressPercent = isBuilding ? (buildProgress / buildTime) * 100f : 0f;
        return $"Build {buildingPrefab.name} ({progressPercent:F0}%)";
    }
    
    public float GetBuildProgress()
    {
        return buildProgress / buildTime;
    }
    
    public Vector3 GetBuildPosition()
    {
        return buildPosition;
    }
}

