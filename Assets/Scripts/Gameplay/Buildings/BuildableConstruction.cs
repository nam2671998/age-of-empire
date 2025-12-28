using UnityEngine;

[RequireComponent(typeof(Health))]
public class BuildableConstruction : MonoBehaviour, IBuildable
{
    [SerializeField] private Transform[] buildPositionTransforms;
    [SerializeField] private GameObject[] progressStates;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
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

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        UpdateHealthState((int)currentHealth, (int)maxHealth);
    }

    public bool Build(float percentage)
    {
        if (IsComplete())
            return false;
            
        if (health == null) return false;
        
        int amount = Mathf.RoundToInt(health.MaxHealth * percentage);
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

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public bool IsComplete()
    {
        if (health == null) return false;
        return health.CurrentHealth >= health.MaxHealth;
    }

    private void OnComplete()
    {
        Debug.Log($"{gameObject.name} has been built fully");
    }

    private void UpdateHealthState(int currentHealth, int maxHealth)
    {
        if (currentHealth >= maxHealth)
        {
            SetCapacityState(2);
            return;
        }
        float amountLeft = currentHealth * 1f / maxHealth;
        if (amountLeft < 0.5f)
        {
            SetCapacityState(0);
        }
        else
        {
            SetCapacityState(1);
        }
    }
    private void SetCapacityState(int state)
    {
        for (int i = 0; i < progressStates.Length; i++)
        {
            progressStates[i].SetActive(i == state);
        }
    }
}
