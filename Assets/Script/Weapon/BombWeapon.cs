using UnityEngine;
using System.Collections.Generic;

public class BombWeapon : BaseWeapon
{
    [Header("피격 넉백")]
    [Tooltip("폭발이 몬스터를 밀어내는 거리입니다.")]
    [SerializeField, Min(0f)] private float explosionKnockbackPower = 15f;
    [Header("Bomb Spawn")]
    [SerializeField] private Transform throwPoint;

    private LineRenderer throwRangeIndicator;
    private LineRenderer explosionRangeIndicator;
    private Material indicatorMaterial;
    private Vector3 aimDirection = Vector3.forward;
    private bool isSelected;
    private float explosionDelayReduction;
    private bool clusterUnlocked;
    private bool megaBlastUnlocked;
    private bool instantFuseUnlocked;

    private float CurrentThrowRange => weaponData != null ? CurrentRange(weaponData.throwRange) : 0f;
    private float CurrentExplosionRadius => weaponData != null
        ? CurrentRange(weaponData.explosionRadius) * (megaBlastUnlocked ? 1.75f : 1f)
        : 0f;
    private float CurrentExplosionDelay => weaponData != null
        ? (instantFuseUnlocked ? 0f : Mathf.Max(0f, weaponData.explosionDelay - explosionDelayReduction))
        : 0f;

    protected override bool CanUpgradeRange => weaponData != null &&
        (weaponData.throwRange > 0f || weaponData.explosionRadius > 0f);

    public override List<WeaponUpgradeType> GetAvailableUpgrades()
    {
        List<WeaponUpgradeType> upgrades = base.GetAvailableUpgrades();
        if (IsFinalUpgradeLevel)
            return upgrades;
        if (weaponData != null && CurrentExplosionDelay > 0f)
            upgrades.Add(WeaponUpgradeType.ExplosionDelay);
        return upgrades;
    }

    protected override bool ApplySpecialUpgrade(WeaponUpgradeType upgrade)
    {
        switch (upgrade)
        {
            case WeaponUpgradeType.ExplosionDelay:
                if (CurrentExplosionDelay <= 0f)
                    return false;
                explosionDelayReduction += 0.2f;
                return true;
            case WeaponUpgradeType.BombCluster:
                clusterUnlocked = true;
                return true;
            case WeaponUpgradeType.BombMegaBlast:
                megaBlastUnlocked = true;
                return true;
            case WeaponUpgradeType.BombInstantFuse:
                instantFuseUnlocked = true;
                return true;
            default:
                return false;
        }
    }

    protected override List<WeaponUpgradeType> GetFinalUpgrades()
    {
        return new List<WeaponUpgradeType>
        {
            WeaponUpgradeType.BombCluster,
            WeaponUpgradeType.BombMegaBlast,
            WeaponUpgradeType.BombInstantFuse
        };
    }

    public override void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selected)
            CreateRangeIndicators();

        if (throwRangeIndicator != null)
            throwRangeIndicator.enabled = selected;
        if (explosionRangeIndicator != null)
            explosionRangeIndicator.enabled = selected;
    }

    public override void SetAimDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
            aimDirection = direction.normalized;
    }

    private void LateUpdate()
    {
        if (!isSelected || weaponData == null || throwRangeIndicator == null)
            return;

        Vector3 origin = (throwPoint != null ? throwPoint.position : transform.position)
            + Vector3.up * 0.08f;
        Vector3 end = origin + aimDirection * CurrentThrowRange;
        throwRangeIndicator.positionCount = 2;
        throwRangeIndicator.SetPosition(0, origin);
        throwRangeIndicator.SetPosition(1, end);
        explosionRangeIndicator.positionCount = 49;

        for (int i = 0; i <= 48; i++)
        {
            float angle = i * Mathf.PI * 2f / 48f;
            Vector3 point = end + new Vector3(
                Mathf.Sin(angle) * CurrentExplosionRadius,
                0f,
                Mathf.Cos(angle) * CurrentExplosionRadius);
            explosionRangeIndicator.SetPosition(i, point);
        }
    }

    private void CreateRangeIndicators()
    {
        if (throwRangeIndicator != null)
            return;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");
        if (shader == null)
            return;

        indicatorMaterial = new Material(shader);
        throwRangeIndicator = CreateRangeLine("Bomb Throw Range", new Color(0.2f, 0.9f, 1f, 0.85f), 0.05f);
        explosionRangeIndicator = CreateRangeLine("Bomb Explosion Range", new Color(1f, 0.4f, 0.1f, 0.9f), 0.07f);
    }

    private LineRenderer CreateRangeLine(string objectName, Color color, float width)
    {
        GameObject lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(transform, false);
        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.widthMultiplier = width;
        line.startColor = color;
        line.endColor = color;
        line.sharedMaterial = indicatorMaterial;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.alignment = LineAlignment.View;
        return line;
    }

    private void OnDestroy()
    {
        if (indicatorMaterial != null)
            Destroy(indicatorMaterial);
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
            Debug.LogError("Bomb Prefab이 연결되지 않았습니다.", this);
            return;
        }

        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.001f)
            return;
        direction.Normalize();

        Vector3 spawnPosition = throwPoint != null ? throwPoint.position : transform.position;
        int bombCount = clusterUnlocked ? 3 : 1;
        for (int i = 0; i < bombCount; i++)
        {
            float spreadAngle = bombCount == 1 ? 0f : (i - 1) * 15f;
            Vector3 bombDirection = Quaternion.Euler(0f, spreadAngle, 0f) * direction;
            GameObject bombObject = Instantiate(
                weaponData.projectilePrefab,
                spawnPosition,
                Quaternion.LookRotation(bombDirection));

            BombProjectile bomb = bombObject.GetComponent<BombProjectile>();
            if (bomb == null)
            {
                Debug.LogError("Bomb Prefab에 BombProjectile 컴포넌트가 필요합니다.", bombObject);
                Destroy(bombObject);
                continue;
            }

            bomb.Initialize(
                bombDirection,
                Damage,
                explosionKnockbackPower,
                weaponData.projectileSpeed,
                CurrentThrowRange,
                CurrentExplosionDelay,
                CurrentExplosionRadius,
                gameObject);
        }
    }
}
