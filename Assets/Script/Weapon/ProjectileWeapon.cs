
using UnityEngine;

public class ProjectileWeapon : BaseWeapon
{
    [Header("Projectile Spawn")]
    [SerializeField] private Transform firePoint;

    public override void Attack(Vector3 direction)
    {
        if (weaponData == null)
        {
            Debug.LogError("WeaponData가 연결되지 않았습니다.", this);
            return;
        }

        if (weaponData.projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab이 연결되지 않았습니다.", this);
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("FirePoint가 연결되지 않았습니다.", this);
            return;
        }

        GameObject projectileObject = Instantiate(
            weaponData.projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        Projectile projectile =
            projectileObject.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                direction,
                weaponData.damage,
                weaponData.projectileSpeed,
                weaponData.projectileLifetime,
                weaponData.knockbackPower
            );
        }
    }
}
