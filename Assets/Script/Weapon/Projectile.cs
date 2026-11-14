
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private Rigidbody rb;

    private Vector3 moveDirection;

    private float damage;
    private float moveSpeed;
    private float lifetime;
    private float knockbackPower;

    private bool isInitialized;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(
        Vector3 direction,
        float damageAmount,
        float speed,
        float lifeTime,
        float knockback)
    {
        moveDirection = direction.normalized;

        damage = damageAmount;
        moveSpeed = speed;
        lifetime = lifeTime;
        knockbackPower = knockback;

        isInitialized = true;

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (!isInitialized || hasHit)
            return;

        Vector3 nextPosition =
            rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit)
            return;

        BaseMonster monster =
            other.GetComponentInParent<BaseMonster>();

        if (monster == null)
            return;

        hasHit = true;

        DamageInfo damageInfo = new DamageInfo
        {
            damage = damage,
            knockbackPower = knockbackPower,
            knockbackDirection = moveDirection
        };

        monster.TakeDamage(damageInfo);

        Destroy(gameObject);
    }
}
