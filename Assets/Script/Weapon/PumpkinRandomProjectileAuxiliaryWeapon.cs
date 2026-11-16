using UnityEngine;

public class PumpkinRandomProjectileAuxiliaryWeapon : AuxiliaryWeapon
{
    private float fireTimer;

    protected override string DefaultDisplayName => "호박 파수탄";
    protected override string DefaultDescription => "호박에서 무작위 방향으로 투사체를 발사합니다.";

    public override System.Collections.Generic.List<AuxiliaryUpgradeType> GetAvailableUpgrades()
    {
        if (Level >= MaxLevel)
            return new System.Collections.Generic.List<AuxiliaryUpgradeType>();
        return new System.Collections.Generic.List<AuxiliaryUpgradeType>
        {
            AuxiliaryUpgradeType.Damage,
            AuxiliaryUpgradeType.AttackSpeed,
            AuxiliaryUpgradeType.ProjectileSpeed,
            AuxiliaryUpgradeType.Range
        };
    }

    public override void Tick(float deltaTime)
    {
        if (targetPumpkin == null)
            return;

        fireTimer += deltaTime;
        if (fireTimer < CurrentInterval(stats.attackInterval))
            return;

        fireTimer = 0f;
        Vector2 random = Random.insideUnitCircle.normalized;
        if (random.sqrMagnitude < 0.01f)
            random = Vector2.up;
        Vector3 direction = new Vector3(random.x, 0f, random.y);
        Vector3 spawnPosition = targetPumpkin.transform.position + Vector3.up * 0.6f;

        GameObject projectileObject = CreateVisual("Pumpkin Random Shot", new Color(1f, 0.45f, 0.15f), 0.32f);
        projectileObject.transform.position = spawnPosition;
        AuxiliaryProjectile projectile = projectileObject.AddComponent<AuxiliaryProjectile>();
        projectile.Initialize(
            direction,
            CurrentSpeed(stats.speed),
            CurrentDuration(stats.duration * RangeMultiplier),
            stats.range,
            CurrentDamage,
            gameObject);
    }
}
