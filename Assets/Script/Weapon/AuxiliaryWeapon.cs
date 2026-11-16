using System.Collections.Generic;
using UnityEngine;

public enum AuxiliaryUpgradeType
{
    Damage,
    Range,
    AttackSpeed,
    ProjectileSpeed,
    Cooldown,
    Duration,
    Knockback,
    OrbitSpeed,
    Recovery
}

public abstract class AuxiliaryWeapon : MonoBehaviour
{
    protected AuxiliaryWeaponStats stats;
    protected Transform owner;
    protected Pumpkin targetPumpkin;
    protected PlayerController player;
    protected bool isEquipped;

    private float damageMultiplier = 1f;
    private float rangeMultiplier = 1f;
    private float intervalMultiplier = 1f;
    private float speedMultiplier = 1f;
    private float durationMultiplier = 1f;
    private float knockbackMultiplier = 1f;
    private float recoveryBonus;

    public int Level { get; private set; } = 1;
    public int MaxLevel => stats != null ? Mathf.Max(1, stats.maxLevel) : 7;
    public string DisplayName => stats != null && !string.IsNullOrWhiteSpace(stats.weaponName)
        ? stats.weaponName
        : DefaultDisplayName;
    public string Description => stats != null && !string.IsNullOrWhiteSpace(stats.description)
        ? stats.description
        : DefaultDescription;
    protected virtual string DefaultDisplayName => GetType().Name;
    protected virtual string DefaultDescription => string.Empty;

    public virtual void Initialize(AuxiliaryWeaponStats weaponStats, Transform weaponOwner, Pumpkin pumpkin, PlayerController playerController)
    {
        stats = weaponStats;
        owner = weaponOwner;
        targetPumpkin = pumpkin;
        player = playerController;
        ResetProgress();
    }

    public virtual void SetEquipped(bool equipped)
    {
        isEquipped = equipped;
    }

    public virtual void Tick(float deltaTime) { }

    public virtual List<AuxiliaryUpgradeType> GetAvailableUpgrades()
    {
        return new List<AuxiliaryUpgradeType>();
    }

    public bool ApplyUpgrade(AuxiliaryUpgradeType upgrade)
    {
        if (Level >= MaxLevel || !GetAvailableUpgrades().Contains(upgrade) || stats == null)
            return false;

        switch (upgrade)
        {
            case AuxiliaryUpgradeType.Damage:
                damageMultiplier *= stats.damageUpgradeMultiplier;
                break;
            case AuxiliaryUpgradeType.Range:
                rangeMultiplier *= stats.rangeUpgradeMultiplier;
                break;
            case AuxiliaryUpgradeType.AttackSpeed:
            case AuxiliaryUpgradeType.Cooldown:
                intervalMultiplier *= stats.intervalUpgradeMultiplier;
                break;
            case AuxiliaryUpgradeType.ProjectileSpeed:
            case AuxiliaryUpgradeType.OrbitSpeed:
                speedMultiplier *= stats.speedUpgradeMultiplier;
                break;
            case AuxiliaryUpgradeType.Duration:
                durationMultiplier *= stats.durationUpgradeMultiplier;
                break;
            case AuxiliaryUpgradeType.Knockback:
                knockbackMultiplier *= stats.knockbackUpgradeMultiplier;
                break;
            case AuxiliaryUpgradeType.Recovery:
                recoveryBonus += stats.recoveryPerUpgrade;
                break;
            default:
                return false;
        }

        Level++;
        return true;
    }

    public string GetUpgradeLabel(AuxiliaryUpgradeType upgrade)
    {
        string amount;
        switch (upgrade)
        {
            case AuxiliaryUpgradeType.Damage: amount = $"공격력 +{(stats.damageUpgradeMultiplier - 1f) * 100f:0}%"; break;
            case AuxiliaryUpgradeType.Range: amount = $"범위 +{(stats.rangeUpgradeMultiplier - 1f) * 100f:0}%"; break;
            case AuxiliaryUpgradeType.AttackSpeed:
            case AuxiliaryUpgradeType.Cooldown: amount = $"간격 -{(1f - stats.intervalUpgradeMultiplier) * 100f:0}%"; break;
            case AuxiliaryUpgradeType.ProjectileSpeed:
            case AuxiliaryUpgradeType.OrbitSpeed: amount = $"속도 +{(stats.speedUpgradeMultiplier - 1f) * 100f:0}%"; break;
            case AuxiliaryUpgradeType.Duration: amount = $"지속 시간 +{(stats.durationUpgradeMultiplier - 1f) * 100f:0}%"; break;
            case AuxiliaryUpgradeType.Knockback: amount = $"넉백 +{(stats.knockbackUpgradeMultiplier - 1f) * 100f:0}%"; break;
            case AuxiliaryUpgradeType.Recovery: amount = $"회복량 +{stats.recoveryPerUpgrade * 100f:0}%"; break;
            default: amount = "효과 강화"; break;
        }
        return amount;
    }

    public void ResetProgress()
    {
        Level = 1;
        damageMultiplier = 1f;
        rangeMultiplier = 1f;
        intervalMultiplier = 1f;
        speedMultiplier = 1f;
        durationMultiplier = 1f;
        knockbackMultiplier = 1f;
        recoveryBonus = 0f;
    }

    protected float CurrentDamage => stats != null ? stats.damage * damageMultiplier : 0f;
    protected float CurrentRange(float baseValue) => baseValue * rangeMultiplier;
    protected float RangeMultiplier => rangeMultiplier;
    protected float CurrentInterval(float baseValue) => Mathf.Max(0.05f, baseValue * intervalMultiplier);
    protected float CurrentSpeed(float baseValue) => baseValue * speedMultiplier;
    protected float CurrentDuration(float baseValue) => baseValue * durationMultiplier;
    protected float CurrentKnockback(float baseValue) => baseValue * knockbackMultiplier;
    protected float RecoveryBonus => recoveryBonus;

    protected GameObject CreateVisual(string objectName, Color color, float size)
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = objectName;
        visual.transform.localScale = Vector3.one * size;
        Collider collider = visual.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        Renderer renderer = visual.GetComponent<Renderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        if (shader != null)
        {
            Material material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            renderer.sharedMaterial = material;
        }

        return visual;
    }

    protected static void DamageMonsters(Vector3 center, float radius, float damage, GameObject source)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius, Physics.AllLayers, QueryTriggerInteraction.Collide);
        HashSet<BaseMonster> hitMonsters = new HashSet<BaseMonster>();
        foreach (Collider hit in hits)
        {
            BaseMonster monster = hit.GetComponentInParent<BaseMonster>();
            if (monster == null || monster.IsDead || !hitMonsters.Add(monster))
                continue;

            Vector3 direction = monster.transform.position - center;
            direction.y = 0f;
            monster.TakeDamage(new DamageInfo
            {
                damage = damage,
                attacker = source,
                attackSource = source,
                knockbackDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward
            });
        }
    }
}
