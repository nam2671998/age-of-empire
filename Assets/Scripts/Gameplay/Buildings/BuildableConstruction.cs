using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(ConstructionVisualController))]
public class BuildableConstruction : MonoBehaviour, IBuildable
{
    [SerializeField] private Transform[] buildPositionTransforms;
    [SerializeField] private ConstructionVisualController visualController;
    [SerializeField] private NavMeshObstacle navMeshObstacle;
    [SerializeField] private Health health;
    [SerializeField] private GameObject beingSabotageFx;
    private SlotReservationController slotReservationController;
    private GridEntity gridEntity;

    private void Awake()
    {
        slotReservationController = new SlotReservationController(buildPositionTransforms);
        if (visualController == null)
        {
            visualController = GetComponent<ConstructionVisualController>();
        }

        TryGetComponent(out gridEntity);
    }

    private void Start()
    {
        if (health != null)
        {
            if (health.CurrentHealth < health.MaxHealth)
                health.HealthChanged += OnBuild;
            else
                Place();
            health.HealthChanged += OnHealthChanged;
        }
        beingSabotageFx.SetActive(false);
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.HealthChanged -= OnHealthChanged;
            health.HealthChanged -= OnBuild;
        }
    }

    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        beingSabotageFx.SetActive(currentHealth < maxHealth);
        if (currentHealth <= 0)
            OnDepleted();
    }
    
    private void OnBuild(int currentHealth, int maxHealth)
    {
        visualController.UpdateBuildState(currentHealth, maxHealth);
        if (currentHealth >= maxHealth)
        {
            health.HealthChanged -= OnBuild;
        }
    }

    public bool Build(int progress)
    {
        if (IsComplete())
            return false;
            
        if (health == null) return false;
        
        int amount = Mathf.Clamp(progress, 0, health.MaxHealth - health.CurrentHealth);
        health.Heal(amount, null);

        if (IsComplete())
        {
            OnComplete();
        }

        return true;
    }

    public Vector3 GetNearestBuildPosition(Vector3 from)
    {
        if (buildPositionTransforms.Length == 0)
            return transform.position;
        Transform nearest = buildPositionTransforms[0];
        for (var i = 1; i < buildPositionTransforms.Length; i++)
        {
            var buildPositionTransform = buildPositionTransforms[i];
            if (Vector3.Distance(buildPositionTransform.position, from) < Vector3.Distance(nearest.position, from))
            {
                nearest = buildPositionTransform;
            }
        }

        return nearest.position;
    }
    
    public bool TryReserveBuildPosition(CommandExecutor executor, out Vector3 position)
    {
        if (slotReservationController == null)
        {
            slotReservationController = new SlotReservationController(buildPositionTransforms);
        }
        else
        {
            slotReservationController.InitializeSlots(buildPositionTransforms);
        }

        bool reserved = slotReservationController.TryReservePosition(executor.GetComponent<IGridEntity>(), out position);
        if (!reserved && executor != null)
        {
            position = GetNearestBuildPosition(executor.transform.position);
        }
        return reserved;
    }

    public void ReleaseBuildPosition(IMovementCapability movementOwner)
    {
        slotReservationController?.ReleasePosition(movementOwner);
    }

    public GameObject GetGameObject()
    {
        try
        {
            return gameObject;
        }
        catch
        {
            return null;
        }
    }

    public bool IsComplete()
    {
        if (health == null) return false;
        return health.CurrentHealth >= health.MaxHealth;
    }

    public void Preview()
    {
        navMeshObstacle.enabled = false;
        if (gridEntity != null)
        {
            gridEntity.enabled = false;
        }
    }

    public void Place()
    {
        navMeshObstacle.enabled = true;
        if (gridEntity != null)
        {
            gridEntity.enabled = true;
        }
    }

    private void OnComplete()
    {
        
    }

    private void OnDepleted()
    {
        gameObject.SetActive(false);
    }
}
