using System.Collections.Generic;
using UnityEngine;

public class AuxiliaryProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float lifetime;
    private float hitRadius;
    private float damage;
    private GameObject source;
    private readonly HashSet<BaseMonster> hitMonsters = new HashSet<BaseMonster>();

    public void Initialize(Vector3 moveDirection, float moveSpeed, float life, float radius, float hitDamage, GameObject owner)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
        lifetime = life;
        hitRadius = radius;
        damage = hitDamage;
        source = owner;
    }

    private void Update()
    {
        transform.position += direction * (speed * Time.deltaTime);
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius, Physics.AllLayers, QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            BaseMonster monster = hit.GetComponentInParent<BaseMonster>();
            if (monster == null || monster.IsDead || !hitMonsters.Add(monster))
                continue;

            monster.TakeDamage(new DamageInfo
            {
                damage = damage,
                attacker = source,
                attackSource = source,
                knockbackDirection = direction
            });
            Destroy(gameObject);
            return;
        }
    }
}
