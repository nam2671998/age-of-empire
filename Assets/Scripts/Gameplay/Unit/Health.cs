using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int startHealth = -1;
    [SerializeField] private bool allowRevive;

    private int currentHealth;

    public event Action<int, int> HealthChanged;
    public event Action<int, GameObject> Damaged;
    public event Action<int, GameObject> Healed;
    public event Action<GameObject> Depleted;

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

    public void Heal(int amount, GameObject source = null)
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
            Healed?.Invoke(applied, source);
            RaiseHealthChanged();
        }
    }

    public void ApplyDamage(int amount, GameObject source = null)
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
            Damaged?.Invoke(applied, source);
            RaiseHealthChanged();
        }

        if (previous > 0 && currentHealth <= 0)
        {
            Deplete(source);
        }
    }

    private void Deplete(GameObject source)
    {
        currentHealth = 0;
        RaiseHealthChanged();
        Depleted?.Invoke(source);
    }

    private void RaiseHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }
}

