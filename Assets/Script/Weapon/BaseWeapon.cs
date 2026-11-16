
using System.Collections.Generic;
using UnityEngine;

public enum WeaponUpgradeType
{
    Damage,
    Range,
    MeleeArc,
    ProjectileSpeed,
    ExplosionDelay
}

public abstract class BaseWeapon : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] protected WeaponData weaponData;

    protected float attackTimer;
    private float damageMultiplier = 1f;
    private float rangeMultiplier = 1f;

    public int Level { get; private set; } = 1;
    public const int MaxLevel = 5;
    public string WeaponName => weaponData != null && !string.IsNullOrWhiteSpace(weaponData.weaponName)
        ? weaponData.weaponName
        : gameObject.name;
    public float Damage => weaponData != null ? weaponData.damage * damageMultiplier : 0f;
    protected float RangeMultiplier => rangeMultiplier;

    public float AttackInterval =>
    weaponData != null ? weaponData.attackInterval : 0.5f;

    public virtual void Initialize(WeaponData data)
    {
        weaponData = data;
        attackTimer = 0f;
        Level = 1;
        damageMultiplier = 1f;
        rangeMultiplier = 1f;
    }

    public virtual List<WeaponUpgradeType> GetAvailableUpgrades()
    {
        List<WeaponUpgradeType> upgrades = new List<WeaponUpgradeType>
        {
            WeaponUpgradeType.Damage
        };

        if (CanUpgradeRange)
            upgrades.Add(WeaponUpgradeType.Range);

        return upgrades;
    }

    public bool ApplyUpgrade(WeaponUpgradeType upgrade)
    {
        if (Level >= MaxLevel || !GetAvailableUpgrades().Contains(upgrade))
            return false;

        switch (upgrade)
        {
            case WeaponUpgradeType.Damage:
                damageMultiplier *= 1.25f;
                break;
            case WeaponUpgradeType.Range:
                rangeMultiplier *= 1.2f;
                break;
            default:
                if (!ApplySpecialUpgrade(upgrade))
                    return false;
                break;
        }

        Level++;
        return true;
    }

    public static string GetUpgradeLabel(WeaponUpgradeType upgrade)
    {
        switch (upgrade)
        {
            case WeaponUpgradeType.Damage: return "공격력 강화 (+25%)";
            case WeaponUpgradeType.Range: return "공격 범위 강화 (+20%)";
            case WeaponUpgradeType.MeleeArc: return "검기 각도 강화 (+15°)";
            case WeaponUpgradeType.ProjectileSpeed: return "투사체 속도 강화 (+20%)";
            case WeaponUpgradeType.ExplosionDelay: return "폭탄 지연 시간 감소 (-0.2초)";
            default: return upgrade.ToString();
        }
    }

    protected virtual bool CanUpgradeRange => weaponData != null && weaponData.attackRange > 0f;

    protected virtual bool ApplySpecialUpgrade(WeaponUpgradeType upgrade)
    {
        return false;
    }

    protected float CurrentRange(float baseRange)
    {
        return baseRange * rangeMultiplier;
    }

    public virtual void SetSelected(bool selected) { }

    public virtual void SetAimDirection(Vector3 direction) { }

    public abstract void Attack(Vector3 direction);
}
