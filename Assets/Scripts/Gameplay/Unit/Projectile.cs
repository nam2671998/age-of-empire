using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float hitDistance = 0.5f;
    
    private IDamageable target;
    private float damage;
    private Unit attacker;
    private Vector3 lastKnownPosition;
    private bool initialized = false;

    public void Initialize(IDamageable target, float damage, Unit attacker)
    {
        this.target = target;
        this.damage = damage;
        this.attacker = attacker;
        
        if (target != null)
        {
            lastKnownPosition = target.GetPosition();
        }
        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        if (target != null && !target.IsDestroyed())
        {
            lastKnownPosition = target.GetPosition();
        }

        Vector3 direction = (lastKnownPosition - transform.position);
        float distance = direction.magnitude;

        if (distance <= hitDistance || distance <= speed * Time.deltaTime)
        {
            Hit();
            return;
        }

        transform.position += direction.normalized * (speed * Time.deltaTime);
        
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void Hit()
    {
        if (target != null && !target.IsDestroyed())
        {
            target.TakeDamage(damage, attacker);
        }
        Destroy(gameObject);
    }
}
