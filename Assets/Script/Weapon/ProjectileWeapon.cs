
using UnityEngine;
using System.Collections.Generic;

public class ProjectileWeapon : BaseWeapon
{
    [Header("Projectile Spawn")]
    [SerializeField] private Transform firePoint;

    private float projectileSpeedMultiplier = 1f;

    public float ProjectileSpeed => weaponData != null ? weaponData.projectileSpeed * projectileSpeedMultiplier : 0f;
    public float ProjectileLifetime => weaponData != null
        ? weaponData.projectileLifetime * RangeMultiplier
        : 0f;

    public override List<WeaponUpgradeType> GetAvailableUpgrades()
    {
        List<WeaponUpgradeType> upgrades = base.GetAvailableUpgrades();
        if (weaponData != null && weaponData.projectileSpeed > 0f)
            upgrades.Add(WeaponUpgradeType.ProjectileSpeed);
        return upgrades;
    }

    protected override bool CanUpgradeRange => weaponData != null && weaponData.projectileLifetime > 0f;

    protected override bool ApplySpecialUpgrade(WeaponUpgradeType upgrade)
    {
        if (upgrade != WeaponUpgradeType.ProjectileSpeed)
            return false;

        projectileSpeedMultiplier *= 1.2f;
        return true;
    }

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
                Damage,
                ProjectileSpeed,
                ProjectileLifetime,
                weaponData.knockbackPower
            );
        }
    }
}
