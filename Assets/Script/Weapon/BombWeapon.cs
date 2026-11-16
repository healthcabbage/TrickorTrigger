using UnityEngine;
using System.Collections.Generic;

public class BombWeapon : BaseWeapon
{
    [Header("Bomb Spawn")]
    [SerializeField] private Transform throwPoint;

    private LineRenderer throwRangeIndicator;
    private LineRenderer explosionRangeIndicator;
    private Material indicatorMaterial;
    private Vector3 aimDirection = Vector3.forward;
    private bool isSelected;
    private float explosionDelayReduction;

    private float CurrentThrowRange => weaponData != null ? CurrentRange(weaponData.throwRange) : 0f;
    private float CurrentExplosionRadius => weaponData != null ? CurrentRange(weaponData.explosionRadius) : 0f;
    private float CurrentExplosionDelay => weaponData != null
        ? Mathf.Max(0f, weaponData.explosionDelay - explosionDelayReduction)
        : 0f;

    protected override bool CanUpgradeRange => weaponData != null &&
        (weaponData.throwRange > 0f || weaponData.explosionRadius > 0f);

    public override List<WeaponUpgradeType> GetAvailableUpgrades()
    {
        List<WeaponUpgradeType> upgrades = base.GetAvailableUpgrades();
        if (weaponData != null && CurrentExplosionDelay > 0f)
            upgrades.Add(WeaponUpgradeType.ExplosionDelay);
        return upgrades;
    }

    protected override bool ApplySpecialUpgrade(WeaponUpgradeType upgrade)
    {
        if (upgrade != WeaponUpgradeType.ExplosionDelay || CurrentExplosionDelay <= 0f)
            return false;

        explosionDelayReduction += 0.2f;
        return true;
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
        GameObject bombObject = Instantiate(
            weaponData.projectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(direction));

        BombProjectile bomb = bombObject.GetComponent<BombProjectile>();
        if (bomb == null)
        {
            Debug.LogError("Bomb Prefab에 BombProjectile 컴포넌트가 필요합니다.", bombObject);
            Destroy(bombObject);
            return;
        }

        bomb.Initialize(
            direction,
            Damage,
            weaponData.knockbackPower,
            weaponData.projectileSpeed,
            CurrentThrowRange,
            CurrentExplosionDelay,
            CurrentExplosionRadius,
            gameObject);
    }
}
