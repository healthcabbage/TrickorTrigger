using UnityEngine;

public class RangedMonster : BaseMonster
{
    [Header("Ranged Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileLifetime = 5f;

    private float attackTimer;

    protected override void UpdateAttack()
    {
        base.UpdateAttack();

        attackTimer += Time.deltaTime;
        if (attackTimer < _monsterStats.attackInterval)
            return;

        attackTimer = 0f;
        Attack();
    }

    private void Attack()
    {
        if (targetPumpkin == null)
            return;

        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab이 연결되지 않았습니다.", this);
            return;
        }

        Vector3 spawnPosition = attackPoint != null ? attackPoint.position : transform.position;
        Vector3 direction = targetPumpkin.transform.position - spawnPosition;
        direction.y = 0f;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        GameObject projectileObject = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(direction));

        MonsterProjectile projectile = projectileObject.GetComponent<MonsterProjectile>();
        if (projectile == null)
        {
            Debug.LogError("Projectile Prefab에 MonsterProjectile 컴포넌트가 필요합니다.", projectileObject);
            Destroy(projectileObject);
            return;
        }

        projectile.Initialize(direction, _monsterStats.attackDamage, projectileSpeed, projectileLifetime);

        Debug.Log(_monsterStats.attackDamage);
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        attackTimer = _monsterStats != null ? _monsterStats.attackInterval : 0f;
    }
}
