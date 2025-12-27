using UnityEngine;

public class BuildableConstruction : MonoBehaviour, IBuildable
{
    [SerializeField] private Transform[] buildPositionTransforms;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject[] progressStates;

    private int currentHealth;

    void Start()
    {
        currentHealth = 0;
        UpdateHealthState();
    }

    public bool Build(float percentage)
    {
        if (IsComplete())
            return false;
            
        int built = Mathf.RoundToInt(maxHealth * percentage);
        currentHealth = Mathf.Clamp(currentHealth + built, 0, maxHealth);
        UpdateHealthState();
        
        Debug.Log($"{gameObject.name} is built and gain {built}. Remaining: {currentHealth}/{maxHealth}");
        
        // Handle depletion
        if (currentHealth >= maxHealth)
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
        return currentHealth >= maxHealth;
    }

    private void OnComplete()
    {
        Debug.Log($"{gameObject.name} has been built fully");
    }

    private void UpdateHealthState()
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
