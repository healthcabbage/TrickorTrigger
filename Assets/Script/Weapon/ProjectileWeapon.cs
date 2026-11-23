
using UnityEngine;
using System.Collections.Generic;

public class ProjectileWeapon : BaseWeapon
{
    [Header("Projectile Spawn")]
    [SerializeField] private Transform firePoint;

    private float projectileSpeedMultiplier = 1f;
    private int projectileCount = 1;
    private int additionalPiercingHits;

    public float ProjectileSpeed => weaponData != null ? weaponData.projectileSpeed * projectileSpeedMultiplier : 0f;
    public float ProjectileLifetime => weaponData != null
        ? weaponData.projectileLifetime * RangeMultiplier
        : 0f;

    public override List<WeaponUpgradeType> GetAvailableUpgrades()
    {
        List<WeaponUpgradeType> upgrades = base.GetAvailableUpgrades();
        if (IsFinalUpgradeLevel)
            return upgrades;
        if (weaponData != null && weaponData.projectileSpeed > 0f)
            upgrades.Add(WeaponUpgradeType.ProjectileSpeed);
        if (weaponData != null && weaponData.attackInterval > 0f)
            upgrades.Add(WeaponUpgradeType.AttackSpeed);
        return upgrades;
    }

    // 원거리 무기는 사거리 대신 공격 속도를 일반 강화 선택지로 제공합니다.
    protected override bool CanUpgradeRange => false;

    protected override bool ApplySpecialUpgrade(WeaponUpgradeType upgrade)
    {
        switch (upgrade)
        {
            case WeaponUpgradeType.ProjectileSpeed:
                projectileSpeedMultiplier *= 1.2f;
                return true;
            case WeaponUpgradeType.ProjectileTripleShot:
                projectileCount = 3;
                return true;
            case WeaponUpgradeType.ProjectilePiercing:
                additionalPiercingHits = 2;
                return true;
            case WeaponUpgradeType.ProjectileRapidFire:
                ReduceAttackInterval(0.75f);
                return true;
            default:
                return false;
        }
    }

    protected override List<WeaponUpgradeType> GetFinalUpgrades()
    {
        return new List<WeaponUpgradeType>
        {
            WeaponUpgradeType.ProjectileTripleShot,
            WeaponUpgradeType.ProjectilePiercing,
            WeaponUpgradeType.ProjectileRapidFire
        };
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

        for (int i = 0; i < projectileCount; i++)
        {
            float spreadAngle = projectileCount == 1 ? 0f : (i - 1) * 12f;
            Vector3 shotDirection = Quaternion.Euler(0f, spreadAngle, 0f) * direction;
            GameObject projectileObject = Instantiate(
                weaponData.projectilePrefab,
                firePoint.position,
                Quaternion.LookRotation(shotDirection));

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(
                    shotDirection,
                    Damage,
                    ProjectileSpeed,
                    ProjectileLifetime,
                    weaponData.knockbackPower,
                    additionalPiercingHits,
                    transform.root.gameObject);
            }
        }
    }
}
