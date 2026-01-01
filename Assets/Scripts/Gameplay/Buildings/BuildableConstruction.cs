using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(ConstructionVisualController))]
public class BuildableConstruction : MonoBehaviour, IBuildable
{
    [SerializeField] private Transform[] buildPositionTransforms;
    [SerializeField] private ConstructionVisualController visualController;
    [SerializeField] NavMeshObstacle navMeshObstacle;

    private Health health;
    private ReservationController reservationController;

    private void Awake()
    {
        health = GetComponent<Health>();
        reservationController = new ReservationController(buildPositionTransforms);
        if (visualController == null)
        {
            visualController = GetComponent<ConstructionVisualController>();
        }
    }

    private void Start()
    {
        if (health != null)
        {
            health.HealthChanged += OnHealthChanged;
            OnHealthChanged(health.CurrentHealth, health.MaxHealth);
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.HealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        visualController.UpdateHealthState(currentHealth, maxHealth);
    }

    public bool Build(int progress)
    {
        if (IsComplete())
            return false;
            
        if (health == null) return false;
        
        int amount = Mathf.Clamp(progress, 0, health.MaxHealth - health.CurrentHealth);
        health.Heal(amount, null);
        
        Debug.Log($"{gameObject.name} is built/repaired by {amount}.");

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
        if (reservationController == null)
        {
            reservationController = new ReservationController(buildPositionTransforms);
        }
        else
        {
            reservationController.SetBuildPositionTransforms(buildPositionTransforms);
        }

        bool reserved = reservationController.TryReservePosition(executor, out position);
        if (!reserved && !ReferenceEquals(executor, null) && executor != null)
        {
            position = GetNearestBuildPosition(executor.transform.position);
        }
        return reserved;
    }

    public void ReleaseBuildPosition(CommandExecutor executor)
    {
        reservationController?.ReleasePosition(executor);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public bool IsComplete()
    {
        if (health == null) return false;
        return health.CurrentHealth >= health.MaxHealth;
    }

    public void Preview()
    {
        navMeshObstacle.enabled = false;
    }

    public void Place()
    {
        navMeshObstacle.enabled = true;
    }

    private void OnComplete()
    {
        Debug.Log($"{gameObject.name} has been built fully");
    }
}
