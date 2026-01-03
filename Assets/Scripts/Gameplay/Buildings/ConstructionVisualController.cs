using UnityEngine;

[DisallowMultipleComponent]
public class ConstructionVisualController : MonoBehaviour
{
    [SerializeField] private GameObject[] progressStates;

    public void UpdateBuildState(int currentHealth, int maxHealth)
    {
        if (maxHealth <= 0)
        {
            SetCapacityState(0);
            return;
        }

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

    public void SetCapacityState(int state)
    {
        if (progressStates == null)
        {
            return;
        }

        for (int i = 0; i < progressStates.Length; i++)
        {
            if (progressStates[i] != null)
            {
                progressStates[i].SetActive(i == state);
            }
        }
    }
}
