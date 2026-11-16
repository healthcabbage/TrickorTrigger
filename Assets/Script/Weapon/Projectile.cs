
using UnityEngine;
using System.Collections.Generic;

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
    private int remainingHits = 1;
    private readonly HashSet<BaseMonster> hitMonsters = new HashSet<BaseMonster>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(
        Vector3 direction,
        float damageAmount,
        float speed,
        float lifeTime,
        float knockback,
        int additionalHits = 0)
    {
        moveDirection = direction.normalized;

        damage = damageAmount;
        moveSpeed = speed;
        lifetime = lifeTime;
        knockbackPower = knockback;
        remainingHits = Mathf.Max(1, additionalHits + 1);

        isInitialized = true;

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (!isInitialized)
            return;

        Vector3 nextPosition =
            rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        BaseMonster monster =
            other.GetComponentInParent<BaseMonster>();

        if (monster == null || !hitMonsters.Add(monster))
            return;

        DamageInfo damageInfo = new DamageInfo
        {
            damage = damage,
            knockbackPower = knockbackPower,
            knockbackDirection = moveDirection
        };

        monster.TakeDamage(damageInfo);

        remainingHits--;
        if (remainingHits <= 0)
            Destroy(gameObject);
    }
}
