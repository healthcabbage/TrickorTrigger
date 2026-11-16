using UnityEngine;

public class OrbitingAttackAuxiliaryWeapon : AuxiliaryWeapon
{
    private GameObject orbitVisual;
    private float orbitAngle;
    private float attackTimer;

    protected override string DefaultDisplayName => "회전 칼날";
    protected override string DefaultDescription => "플레이어 주변을 돌며 닿은 적을 공격합니다.";

    public override System.Collections.Generic.List<AuxiliaryUpgradeType> GetAvailableUpgrades()
    {
        if (Level >= MaxLevel)
            return new System.Collections.Generic.List<AuxiliaryUpgradeType>();
        return new System.Collections.Generic.List<AuxiliaryUpgradeType>
        {
            AuxiliaryUpgradeType.Damage,
            AuxiliaryUpgradeType.Range,
            AuxiliaryUpgradeType.AttackSpeed,
            AuxiliaryUpgradeType.OrbitSpeed
        };
    }

    public override void SetEquipped(bool equipped)
    {
        base.SetEquipped(equipped);
        if (equipped && orbitVisual == null)
            orbitVisual = CreateVisual("Orbiting Auxiliary Blade", new Color(0.25f, 0.9f, 1f), 0.42f);
        if (orbitVisual != null)
            orbitVisual.SetActive(equipped);
    }

    public override void Tick(float deltaTime)
    {
        if (owner == null || orbitVisual == null)
            return;

        orbitAngle += CurrentSpeed(stats.orbitSpeed) * deltaTime;
        Vector3 offset = Quaternion.Euler(0f, orbitAngle, 0f) * Vector3.forward * CurrentRange(stats.orbitRadius);
        Vector3 hitPosition = owner.position + Vector3.up * 0.75f + offset;
        orbitVisual.transform.position = hitPosition;

        attackTimer += deltaTime;
        if (attackTimer < CurrentInterval(stats.attackInterval))
            return;

        attackTimer = 0f;
        DamageMonsters(hitPosition, CurrentRange(stats.range), CurrentDamage, gameObject);
    }
}
