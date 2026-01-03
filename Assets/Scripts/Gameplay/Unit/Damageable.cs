using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Damageable : MonoBehaviour, IDamageable
{
    [SerializeField] private Faction faction = Faction.Neutral;
    [SerializeField] private bool destroyOnDeath = true;

    public event Action OnDeath;
    public event Action OnDamageTakenHandler;
    public event Action<int, int> OnHealthChangedHandler;
    public event Action<int> OnDamagedHandler;
    public event Action<int> OnHealedHandler;

    private Health health;

    public bool IsDead => health != null && health.IsDepleted;
    public Faction Faction
    {
        get => faction;
        set => faction = value;
    }

    private void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            health.HealthChanged += OnHealthChanged;
            health.Damaged += OnDamaged;
            health.Healed += OnHealed;
            health.Depleted += OnDepleted;

            OnHealthChanged(health.CurrentHealth, health.MaxHealth);
        }
    }

    public void Heal(int amount)
    {
        if (health == null)
        {
            return;
        }

        health.Heal(amount);
    }

    public void TakeDamage(int damage)
    {
        if (health == null)
        {
            return;
        }

        health.ApplyDamage(damage);

        if (damage > 0f)
        {
            OnDamageTakenHandler?.Invoke();
        }
    }

    public float GetHealth() => health != null ? health.CurrentHealth : 0f;

    public float GetMaxHealth() => health != null ? health.MaxHealth : 0f;

    public bool IsDestroyed() => health != null && health.IsDepleted;

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

    public Vector3 GetPosition() => transform.position;

    public void SetFaction(Faction newFaction)
    {
        faction = newFaction;
    }

    private void OnHealthChanged(int current, int max)
    {
        OnHealthChangedHandler?.Invoke(current, max);
    }

    private void OnDamaged(int amount)
    {
        OnDamagedHandler?.Invoke(amount);
    }

    private void OnHealed(int amount)
    {
        OnHealedHandler?.Invoke(amount);
    }

    private void OnDepleted()
    {
        OnDeath?.Invoke();

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
}
