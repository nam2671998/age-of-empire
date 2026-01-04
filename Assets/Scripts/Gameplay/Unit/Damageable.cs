using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Damageable : MonoBehaviour, IDamageable
{
    [SerializeField] private Faction faction = Faction.Neutral;
    [SerializeField] private Collider hitCollider;
    [SerializeField] private GameObject hitFx;

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
    public Collider HitCollider => hitCollider;

    private void Awake()
    {
        if (hitCollider == null)
        {
            TryGetComponent(out hitCollider);
        }

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
            ObjectPool.Spawn(hitFx, hitCollider.bounds.center);
        }
    }

    public bool IsDestroyed()
    {
        if (this != null)
        {
            return health != null && health.IsDepleted;
        }
        return true;
    }

    public GameObject GetGameObject()
    {
        if (this != null)
        {
            return gameObject;
        }
        return null;
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
    }
}
