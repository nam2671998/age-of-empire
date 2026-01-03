using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int startHealth = -1;
    [SerializeField] private bool allowRevive;

    private int currentHealth;

    public event Action<int, int> HealthChanged;
    public event Action<int> Damaged;
    public event Action<int> Healed;
    public event Action Depleted;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDepleted => CurrentHealth <= 0;

    private void Awake()
    {
        if (maxHealth < 0)
        {
            maxHealth = 0;
        }

        currentHealth = Mathf.Clamp(startHealth, 0, maxHealth);

        RaiseHealthChanged();
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (IsDepleted && !allowRevive)
        {
            return;
        }

        int previous = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        int applied = currentHealth - previous;
        if (applied > 0)
        {
            Healed?.Invoke(applied);
            RaiseHealthChanged();
        }
    }

    public void ApplyDamage(int amount)
    {
        if (IsDepleted)
        {
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        int previous = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);

        int applied = previous - currentHealth;
        if (applied > 0)
        {
            Damaged?.Invoke(applied);
            RaiseHealthChanged();
        }

        if (previous > 0 && currentHealth <= 0)
        {
            Deplete();
        }
    }

    private void Deplete()
    {
        currentHealth = 0;
        RaiseHealthChanged();
        Depleted?.Invoke();
    }

    private void RaiseHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }
}

