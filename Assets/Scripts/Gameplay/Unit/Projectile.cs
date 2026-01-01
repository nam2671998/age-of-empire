using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float hitDistance = 0.5f;
    
    private IDamageable target;
    private int damage;
    private Unit attacker;
    private Vector3 lastKnownPosition;
    private bool initialized = false;

    private void OnEnable()
    {
        ResetState();
    }

    public void Initialize(IDamageable target, int damage, Unit attacker)
    {
        this.target = target;
        this.damage = damage;
        this.attacker = attacker;
        initialized = true;
        
        if (target != null)
        {
            lastKnownPosition = target.GetPosition();
        }
        else
        {
            lastKnownPosition = transform.position;
        }
    }

    void Update()
    {
        if (!initialized) return;

        if (target == null || target.GetGameObject() == null)
        {
            this.Recycle();
            return;
        }

        if (!target.IsDestroyed())
        {
            lastKnownPosition = target.GetPosition();
        }
        Vector3 direction = (lastKnownPosition - transform.position);
        direction.y = 0f;
        float distance = direction.magnitude;

        if (distance <= hitDistance || distance <= speed * Time.deltaTime)
        {
            Hit();
            return;
        }

        transform.position = transform.position + direction.normalized * (speed * Time.deltaTime);
        
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
        this.Recycle();
    }

    private void ResetState()
    {
        initialized = false;
        target = null;
        damage = 0;
        attacker = null;
        lastKnownPosition = transform.position;
    }
}
