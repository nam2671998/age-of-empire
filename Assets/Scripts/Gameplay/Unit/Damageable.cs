using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Damageable : MonoBehaviour, IDamageable
{
    [Header("Faction")]
    [SerializeField] private Faction faction = global::Faction.Neutral;

    [SerializeField] private bool destroyOnDeath = true;

    public event Action OnDeath;
    public event Action<float> OnHealthChanged;
    public event Action<Unit> OnDamageTaken;
    public event Action<float, float> HealthChanged;
    public event Action<float, Unit> Damaged;
    public event Action<float> Healed;
    public event Action<Unit> Depleted;

    private Health health;

    public bool IsDead => health != null && health.IsDepleted;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            health.HealthChanged += OnUnderlyingHealthChanged;
            health.Damaged += OnUnderlyingDamaged;
            health.Healed += OnUnderlyingHealed;
            health.Depleted += OnUnderlyingDepleted;

            OnUnderlyingHealthChanged(health.CurrentHealth, health.MaxHealth);
        }
    }

    public void Heal(float amount)
    {
        if (health == null)
        {
            return;
        }

        health.Heal(amount, null);
    }

    public void TakeDamage(float damage, Unit attacker)
    {
        if (health == null)
        {
            return;
        }

        health.ApplyDamage(damage, attacker != null ? attacker.gameObject : null);

        if (damage > 0f)
        {
            OnDamageTaken?.Invoke(attacker);
        }
    }

    public float GetHealth() => health != null ? health.CurrentHealth : 0f;

    public float GetMaxHealth() => health != null ? health.MaxHealth : 0f;

    public bool IsDestroyed() => health != null && health.IsDepleted;

    public GameObject GetGameObject() => gameObject;

    public Vector3 GetPosition() => transform.position;

    public Faction Faction => faction;

    public void SetFaction(Faction newFaction)
    {
        faction = newFaction;
    }

    private void OnUnderlyingHealthChanged(float current, float max)
    {
        OnHealthChanged?.Invoke(current);
        HealthChanged?.Invoke(current, max);
    }

    private void OnUnderlyingDamaged(float amount, GameObject source)
    {
        Unit attacker = source != null ? source.GetComponent<Unit>() : null;
        Damaged?.Invoke(amount, attacker);
    }

    private void OnUnderlyingHealed(float amount, GameObject source)
    {
        Healed?.Invoke(amount);
    }

    private void OnUnderlyingDepleted(GameObject source)
    {
        Unit attacker = source != null ? source.GetComponent<Unit>() : null;
        Depleted?.Invoke(attacker);
        OnDeath?.Invoke();

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
}
