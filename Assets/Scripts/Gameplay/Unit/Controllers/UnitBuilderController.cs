using UnityEngine;

public class UnitBuilderController : MonoBehaviour, IBuildCapability
{
    [Header("Building Settings")]
    [SerializeField] private float buildRange = 3f;
    
    private Vector3 buildTargetPosition;
    private GameObject buildTargetPrefab;
    private bool isBuilding = false;
    
    public bool IsBuilding => isBuilding;
    public float BuildRange => buildRange;
    public Vector3 BuildTargetPosition => buildTargetPosition;
    
    bool IBuildCapability.IsBuilding() => isBuilding;
    
    public void SetBuildTarget(Vector3 position, GameObject buildingPrefab)
    {
        if (TryGetComponent(out SettlerUnit settler))
        {
            settler.StopOtherActions();
        }
        
        buildTargetPosition = position;
        buildTargetPrefab = buildingPrefab;
        
        if (TryGetComponent(out IMovementCapability movement))
        {
            movement.MoveTo(position, buildRange * 0.8f);
        }
    }
    
    public void StartBuilding()
    {
        isBuilding = true;
        
        if (TryGetComponent(out IMovementCapability movement))
        {
            movement.StopMovement();
        }
    }
    
    public void StopBuilding()
    {
        isBuilding = false;
    }
    
    public bool IsInRange()
    {
        float distance = Vector3.Distance(transform.position, buildTargetPosition);
        return distance <= buildRange;
    }
    
    public GameObject GetBuildingPrefab()
    {
        return buildTargetPrefab;
    }
}

