using UnityEngine;

public class PumpkinShockwaveAuxiliaryWeapon : AuxiliaryWeapon
{
    private GameObject orbitVisual;
    private float orbitAngle;
    private float pulseTimer;

    protected override string DefaultDisplayName => "호박 충격파";
    protected override string DefaultDescription => "호박에서 바깥으로 퍼지는 충격파가 적을 밀쳐냅니다.";

    public override System.Collections.Generic.List<AuxiliaryUpgradeType> GetAvailableUpgrades()
    {
        if (Level >= MaxLevel)
            return new System.Collections.Generic.List<AuxiliaryUpgradeType>();
        return new System.Collections.Generic.List<AuxiliaryUpgradeType>
        {
            AuxiliaryUpgradeType.Cooldown,
            AuxiliaryUpgradeType.Range,
            AuxiliaryUpgradeType.Knockback,
            AuxiliaryUpgradeType.OrbitSpeed
        };
    }

    public override void SetEquipped(bool equipped)
    {
        base.SetEquipped(equipped);
        if (equipped && orbitVisual == null)
            orbitVisual = CreateVisual("Pumpkin Shockwave Core", new Color(0.7f, 0.45f, 1f), 0.48f);
        if (orbitVisual != null)
            orbitVisual.SetActive(equipped);
    }

    public override void Tick(float deltaTime)
    {
        if (targetPumpkin == null || orbitVisual == null)
            return;

        Vector3 center = targetPumpkin.transform.position;
        orbitAngle += CurrentSpeed(stats.orbitSpeed) * deltaTime;
        Vector3 offset = Quaternion.Euler(0f, orbitAngle, 0f) * Vector3.forward * CurrentRange(stats.orbitRadius);
        orbitVisual.transform.position = center + Vector3.up * 0.65f + offset;

        pulseTimer += deltaTime;
        if (pulseTimer < CurrentInterval(stats.attackInterval))
            return;

        pulseTimer = 0f;
        AuxiliaryShockwaveVisual.Show(
            center,
            CurrentRange(stats.range),
            CurrentDuration(stats.duration),
            CurrentKnockback(stats.knockbackPower));
    }
}
