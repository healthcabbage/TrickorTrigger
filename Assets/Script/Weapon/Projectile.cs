
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
    private GameObject owner;

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
        int additionalHits = 0,
        GameObject projectileOwner = null)
    {
        moveDirection = direction.normalized;

        damage = damageAmount;
        moveSpeed = speed;
        lifetime = lifeTime;
        knockbackPower = knockback;
        owner = projectileOwner;
        remainingHits = Mathf.Max(1, additionalHits + 1);

        isInitialized = true;

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (!isInitialized)
            return;

        float stepDistance = moveSpeed * Time.fixedDeltaTime;
        float projectileRadius = 0.05f;
        Collider projectileCollider = GetComponent<Collider>();
        if (projectileCollider is SphereCollider sphereCollider)
            projectileRadius = sphereCollider.radius * Mathf.Max(
                transform.lossyScale.x,
                Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));

        // MovePosition can skip thin enemy colliders at high speed. Sweep the
        // projectile volume first, then process the hit through the same path
        // used by trigger callbacks.
        RaycastHit[] sweptHits = Physics.SphereCastAll(
            rb.position,
            projectileRadius,
            moveDirection,
            stepDistance,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide);
        foreach (RaycastHit sweptHit in sweptHits)
        {
            if (TryHitMonster(sweptHit.collider))
                return;
        }

        Vector3 nextPosition = rb.position + moveDirection * stepDistance;

        rb.MovePosition(nextPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHitMonster(other);
    }

    private bool TryHitMonster(Collider other)
    {
        if (owner != null && (other.transform == owner.transform ||
                              other.transform.IsChildOf(owner.transform)))
            return false;

        BaseMonster monster = other.GetComponentInParent<BaseMonster>();

        if (monster == null || !hitMonsters.Add(monster))
            return false;

        DamageInfo damageInfo = new DamageInfo
        {
            damage = damage,
            knockbackPower = knockbackPower,
            knockbackDirection = moveDirection
        };

        monster.TakeDamage(damageInfo);

        remainingHits--;
        if (remainingHits <= 0)
        {
            Destroy(gameObject);
            return true;
        }

        return false;
    }
}
