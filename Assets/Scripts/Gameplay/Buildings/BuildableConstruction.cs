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
    [SerializeField] private GridEntity gridEntity;
    [SerializeField] private bool isBuilt;
    private SlotReservationController slotReservationController;

    private void Awake()
    {
        slotReservationController = new SlotReservationController(buildPositionTransforms);
        if (visualController == null)
        {
            visualController = GetComponent<ConstructionVisualController>();
        }
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
            health.Damaged += OnDamaged;
        }
        beingSabotageFx.SetActive(false);
    }

    private void OnDamaged(int damage, GameObject attacker)
    {
        if (damage > 0)
            beingSabotageFx.SetActive(true);
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
        if (currentHealth >= maxHealth)
            beingSabotageFx.SetActive(false);
        if (currentHealth <= 0)
            OnDepleted();
    }
    
    private void OnBuild(int currentHealth, int maxHealth)
    {
        visualController.UpdateBuildState(currentHealth, maxHealth);
        if (currentHealth >= maxHealth)
        {
            health.HealthChanged -= OnBuild;
            isBuilt = true;
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
        return isBuilt;
    }

    public void Preview()
    {
        navMeshObstacle.enabled = false;
        gridEntity.enabled = false;
        isBuilt = false;
    }

    public void Place()
    {
        navMeshObstacle.enabled = true;
        gridEntity.enabled = true;
        if (!isBuilt)
            visualController.SetCapacityState(0);
        if (TryGetComponent(out CameraFocusEntity entity) && TryGetComponent(out IFactionOwner factionOwner) && factionOwner.Faction == Faction.Player1)
        {
            entity.SubscribeFocus();
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
