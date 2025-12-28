using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float startHealth = -1f;
    [SerializeField] private bool allowRevive;

    private float currentHealth;

    public event Action<float, float> HealthChanged;
    public event Action<float, GameObject> Damaged;
    public event Action<float, GameObject> Healed;
    public event Action<GameObject> Depleted;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDepleted => CurrentHealth <= 0;

    private void Awake()
    {
        if (maxHealth < 0f)
        {
            maxHealth = 0f;
        }

        currentHealth = Mathf.Clamp(startHealth, 0f, maxHealth);

        RaiseHealthChanged();
    }

    public void SetMaxHealth(float newMaxHealth, bool keepCurrentPercent = true)
    {
        if (newMaxHealth < 0f)
        {
            newMaxHealth = 0f;
        }

        float previousMax = maxHealth;
        float previousCurrent = currentHealth;

        maxHealth = newMaxHealth;

        if (keepCurrentPercent && previousMax > 0f)
        {
            float percent = previousCurrent / previousMax;
            currentHealth = Mathf.Clamp(maxHealth * percent, 0f, maxHealth);
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        RaiseHealthChanged();
    }

    public void SetHealth(float value, GameObject source = null)
    {
        if (value < 0f)
        {
            value = 0f;
        }

        float previous = currentHealth;

        currentHealth = Mathf.Clamp(value, 0f, maxHealth);

        if (!Mathf.Approximately(previous, currentHealth))
        {
            RaiseHealthChanged();
        }

        if (previous > 0 && currentHealth <= 0f)
        {
            Deplete(source);
        }
    }

    public void Heal(float amount, GameObject source = null)
    {
        if (amount <= 0f)
        {
            return;
        }

        if (IsDepleted && !allowRevive)
        {
            return;
        }

        float previous = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);

        float applied = currentHealth - previous;
        if (applied > 0f)
        {
            Healed?.Invoke(applied, source);
            RaiseHealthChanged();
        }
    }

    public void ApplyDamage(float amount, GameObject source = null)
    {
        if (IsDepleted)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        float previous = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);

        float applied = previous - currentHealth;
        if (applied > 0f)
        {
            Damaged?.Invoke(applied, source);
            RaiseHealthChanged();
        }

        if (previous > 0 && currentHealth <= 0f)
        {
            Deplete(source);
        }
    }

    private void Deplete(GameObject source)
    {
        currentHealth = 0f;
        RaiseHealthChanged();
        Depleted?.Invoke(source);
    }

    private void RaiseHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }
}

