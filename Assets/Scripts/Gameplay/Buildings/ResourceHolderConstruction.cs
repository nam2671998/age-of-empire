using UnityEngine;

[DisallowMultipleComponent]
public sealed class ResourceHolderConstruction : MonoBehaviour, IResourceHolderConstruction
{
    [SerializeField] private int priority = 0;
    [SerializeField] private Faction faction = Faction.Player1;
    [SerializeField] private Transform depositPoint;
    [SerializeField] private float depositRadius = 1f;
    [SerializeField] private Health health;

    private IBuildable buildable;
    private bool registered;

    public int Priority => priority;
    public Faction Faction => faction;

    private void Awake()
    {
        buildable = GetComponentInChildren<IBuildable>(true);
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.HealthChanged += OnHealthChanged;
        }

        TryRegisterIfBuilt();
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.HealthChanged -= OnHealthChanged;
        }

        Unregister();
    }

    private void OnDestroy()
    {
        Unregister();
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public Vector3 GetDepositPosition()
    {
        return depositPoint != null ? depositPoint.position : transform.position;
    }

    public float GetDepositRadius()
    {
        return depositRadius;
    }

    private void OnHealthChanged(int current, int max)
    {
        TryRegisterIfBuilt();
        if (current <= 0)
        {
            Unregister();
        }
    }

    private void TryRegisterIfBuilt()
    {
        if (registered)
        {
            return;
        }

        if (buildable != null && !buildable.IsComplete())
        {
            return;
        }

        ResourceHolderConstructionRegistry.Register(this);
        registered = true;
    }

    private void Unregister()
    {
        if (!registered)
        {
            return;
        }

        ResourceHolderConstructionRegistry.Unregister(this);
        registered = false;
    }
}

