using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MonsterProjectile : MonoBehaviour
{
    private Rigidbody body;
    private Vector3 direction;
    private float damage;
    private float speed;
    private float lifetime;
    private bool initialized;
    private bool hasHit;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 moveDirection, float damageAmount, float moveSpeed, float lifeTime)
    {
        direction = moveDirection.normalized;
        damage = damageAmount;
        speed = moveSpeed;
        lifetime = lifeTime;
        initialized = true;
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (!initialized || hasHit)
            return;

        body.MovePosition(body.position + direction * speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit)
            return;

        Pumpkin pumpkin = other.GetComponentInParent<Pumpkin>();
        if (pumpkin == null)
            return;

        hasHit = true;
        pumpkin.TakeDamage(damage);
        Destroy(gameObject);
    }
}
