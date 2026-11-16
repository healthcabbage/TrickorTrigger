
using System.Collections.Generic;
using UnityEngine;

public enum WeaponUpgradeType
{
    Damage,
    Range,
    AttackSpeed,
    MeleeArc,
    ProjectileSpeed,
    ExplosionDelay,
    ProjectileTripleShot,
    ProjectilePiercing,
    ProjectileRapidFire,
    MeleeBleed,
    MeleeSoulHarvest,
    MeleeExtraStrike,
    BombCluster,
    BombMegaBlast,
    BombInstantFuse
}

public abstract class BaseWeapon : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] protected WeaponData weaponData;

    protected float attackTimer;
    private float damageMultiplier = 1f;
    private float rangeMultiplier = 1f;
    private float attackIntervalMultiplier = 1f;

    public int Level { get; private set; } = 1;
    public const int MaxLevel = 7;
    protected bool IsFinalUpgradeLevel => Level == MaxLevel - 1;
    public string WeaponName => weaponData != null && !string.IsNullOrWhiteSpace(weaponData.weaponName)
        ? weaponData.weaponName
        : gameObject.name;
    public float Damage => weaponData != null ? weaponData.damage * damageMultiplier : 0f;
    protected float RangeMultiplier => rangeMultiplier;

    public float AttackInterval =>
    weaponData != null ? weaponData.attackInterval * attackIntervalMultiplier : 0.5f;

    public virtual void Initialize(WeaponData data)
    {
        weaponData = data;
        attackTimer = 0f;
        Level = 1;
        damageMultiplier = 1f;
        rangeMultiplier = 1f;
        attackIntervalMultiplier = 1f;
    }

    public virtual List<WeaponUpgradeType> GetAvailableUpgrades()
    {
        if (Level == MaxLevel - 1)
            return GetFinalUpgrades();

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
            case WeaponUpgradeType.AttackSpeed:
                attackIntervalMultiplier *= 0.9f;
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
            case WeaponUpgradeType.AttackSpeed: return "공격 속도 강화 (간격 -10%)";
            case WeaponUpgradeType.MeleeArc: return "검기 각도 강화 (+15°)";
            case WeaponUpgradeType.ProjectileSpeed: return "투사체 속도 강화 (+20%)";
            case WeaponUpgradeType.ExplosionDelay: return "폭탄 지연 시간 감소 (-0.2초)";
            case WeaponUpgradeType.ProjectileTripleShot: return "특수 · 삼중 사격\n세 방향으로 투사체 발사";
            case WeaponUpgradeType.ProjectilePiercing: return "특수 · 관통 탄환\n투사체가 적 3마리를 관통";
            case WeaponUpgradeType.ProjectileRapidFire: return "특수 · 속사\n공격 간격 25% 감소";
            case WeaponUpgradeType.MeleeBleed: return "특수 · 출혈 검기\n3초간 매초 공격력의 15% 피해";
            case WeaponUpgradeType.MeleeSoulHarvest: return "특수 · 영혼 흡수\n처치 시 20% 확률로 영혼 생성, 흡수 시 체력 10% 회복";
            case WeaponUpgradeType.MeleeExtraStrike: return "특수 · 연속 베기\n공격마다 60% 피해의 추가 타격 1회";
            case WeaponUpgradeType.BombCluster: return "특수 · 산탄 폭탄\n세 방향으로 폭탄 투척";
            case WeaponUpgradeType.BombMegaBlast: return "특수 · 대폭발\n폭발 반경 75% 증가";
            case WeaponUpgradeType.BombInstantFuse: return "특수 · 즉발 신관\n도달 후 바로 폭발";
            default: return upgrade.ToString();
        }
    }

    protected virtual bool CanUpgradeRange => weaponData != null && weaponData.attackRange > 0f;

    protected virtual bool ApplySpecialUpgrade(WeaponUpgradeType upgrade)
    {
        return false;
    }

    protected virtual List<WeaponUpgradeType> GetFinalUpgrades()
    {
        return new List<WeaponUpgradeType>();
    }

    protected void IncreaseDamage(float multiplier) => damageMultiplier *= multiplier;
    protected void IncreaseRange(float multiplier) => rangeMultiplier *= multiplier;
    protected void ReduceAttackInterval(float multiplier) => attackIntervalMultiplier *= multiplier;

    protected float CurrentRange(float baseRange)
    {
        return baseRange * rangeMultiplier;
    }

    public virtual void SetSelected(bool selected) { }

    public virtual void SetAimDirection(Vector3 direction) { }

    public abstract void Attack(Vector3 direction);
}
