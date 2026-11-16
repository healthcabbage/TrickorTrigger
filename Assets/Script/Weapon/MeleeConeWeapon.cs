using System.Collections.Generic;
using UnityEngine;

public class MeleeConeWeapon : BaseWeapon
{
    [Header("Attack Origin")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask monsterLayers = ~0;

    private LineRenderer rangeIndicator;
    private LineRenderer slashTrail;
    private LineRenderer slashGlow;
    private Material indicatorMaterial;
    private Vector3 aimDirection = Vector3.forward;
    private bool isSelected;
    private bool isSlashing;
    private bool reverseNextSlash;
    private float slashTimer;
    private Vector3 slashDirection;
    private float attackAngleBonus;
    private const float SlashDuration = 0.2f;

    private float CurrentAttackRange => weaponData != null
        ? CurrentRange(weaponData.attackRange)
        : 0f;
    private float CurrentAttackAngle => weaponData != null
        ? Mathf.Min(180f, weaponData.attackAngle + attackAngleBonus)
        : 0f;

    protected override bool CanUpgradeRange => weaponData != null && weaponData.attackRange > 0f;

    public override List<WeaponUpgradeType> GetAvailableUpgrades()
    {
        List<WeaponUpgradeType> upgrades = base.GetAvailableUpgrades();
        if (CurrentAttackAngle < 180f)
            upgrades.Add(WeaponUpgradeType.MeleeArc);
        return upgrades;
    }

    protected override bool ApplySpecialUpgrade(WeaponUpgradeType upgrade)
    {
        if (upgrade != WeaponUpgradeType.MeleeArc || CurrentAttackAngle >= 180f)
            return false;

        attackAngleBonus += Mathf.Min(15f, 180f - CurrentAttackAngle);
        return true;
    }

    public override void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selected)
            CreateRangeIndicator();

        if (rangeIndicator != null)
            rangeIndicator.enabled = selected;
    }

    public override void SetAimDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
            aimDirection = direction.normalized;
    }

    private void LateUpdate()
    {
        if (weaponData == null)
            return;

        Vector3 origin = (attackPoint != null ? attackPoint.position : transform.position)
            + Vector3.up * 0.08f;

        if (isSelected && rangeIndicator != null)
        {
            const int segments = 32;
            Vector3[] points = new Vector3[segments + 3];
            points[0] = origin;
            float halfAngle = CurrentAttackAngle * 0.5f;
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);
                points[i + 1] = origin + Quaternion.Euler(0f, angle, 0f)
                    * aimDirection * CurrentAttackRange;
            }
            points[points.Length - 1] = origin;
            rangeIndicator.positionCount = points.Length;
            rangeIndicator.SetPositions(points);
        }

        UpdateSlashTrail(origin);
    }

    private void CreateRangeIndicator()
    {
        if (rangeIndicator != null)
            return;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");
        if (shader == null)
            return;

        indicatorMaterial = new Material(shader);
        GameObject lineObject = new GameObject("Melee Attack Range");
        lineObject.transform.SetParent(transform, false);
        rangeIndicator = lineObject.AddComponent<LineRenderer>();
        rangeIndicator.useWorldSpace = true;
        rangeIndicator.widthMultiplier = 0.05f;
        rangeIndicator.startColor = new Color(1f, 0.8f, 0.1f, 0.85f);
        rangeIndicator.endColor = rangeIndicator.startColor;
        rangeIndicator.sharedMaterial = indicatorMaterial;
        rangeIndicator.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rangeIndicator.receiveShadows = false;
        rangeIndicator.alignment = LineAlignment.View;

        GameObject slashObject = new GameObject("Melee Sword Slash");
        slashObject.transform.SetParent(transform, false);
        slashTrail = slashObject.AddComponent<LineRenderer>();
        slashTrail.useWorldSpace = true;
        slashTrail.widthMultiplier = 0.1f;
        slashTrail.numCornerVertices = 4;
        slashTrail.startColor = new Color(1f, 1f, 0.95f, 1f);
        slashTrail.endColor = new Color(1f, 0.75f, 0.2f, 0.85f);
        slashTrail.sharedMaterial = indicatorMaterial;
        slashTrail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        slashTrail.receiveShadows = false;
        slashTrail.alignment = LineAlignment.View;
        slashTrail.enabled = false;

        GameObject glowObject = new GameObject("Melee Slash Glow");
        glowObject.transform.SetParent(transform, false);
        slashGlow = glowObject.AddComponent<LineRenderer>();
        slashGlow.useWorldSpace = true;
        slashGlow.widthMultiplier = 0.24f;
        slashGlow.numCornerVertices = 5;
        slashGlow.startColor = new Color(1f, 0.35f, 0.05f, 0.5f);
        slashGlow.endColor = new Color(1f, 0.75f, 0.1f, 0.25f);
        slashGlow.sharedMaterial = indicatorMaterial;
        slashGlow.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        slashGlow.receiveShadows = false;
        slashGlow.alignment = LineAlignment.View;
        slashGlow.enabled = false;

    }

    private void BeginSlash(Vector3 direction)
    {
        if (slashTrail == null)
            CreateRangeIndicator();

        if (slashTrail == null)
            return;

        slashDirection = direction.normalized;
        slashTimer = 0f;
        isSlashing = true;
        reverseNextSlash = !reverseNextSlash;
        slashTrail.enabled = true;
        slashGlow.enabled = true;
    }

    private void UpdateSlashTrail(Vector3 origin)
    {
        if (!isSlashing || slashTrail == null)
            return;

        slashTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(slashTimer / SlashDuration);
        float halfAngle = CurrentAttackAngle * 0.5f;
        float startAngle = reverseNextSlash ? -halfAngle : halfAngle;
        float endAngle = reverseNextSlash ? -halfAngle + CurrentAttackAngle * progress
            : halfAngle - CurrentAttackAngle * progress;

        const int segments = 20;
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, i / (float)segments);
            points[i] = origin + Quaternion.Euler(0f, angle, 0f)
                * slashDirection * CurrentAttackRange;
        }
        slashTrail.positionCount = points.Length;
        slashTrail.SetPositions(points);
        slashGlow.positionCount = points.Length;
        slashGlow.SetPositions(points);

        if (progress >= 1f)
        {
            isSlashing = false;
            slashTrail.enabled = false;
            slashGlow.enabled = false;
        }
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

        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.001f)
            return;
        direction.Normalize();
        BeginSlash(direction);

        Vector3 origin = attackPoint != null ? attackPoint.position : transform.position;
        Collider[] hits = Physics.OverlapSphere(
            origin,
            CurrentAttackRange,
            monsterLayers,
            QueryTriggerInteraction.Collide);

        HashSet<BaseMonster> hitMonsters = new HashSet<BaseMonster>();
        foreach (Collider hit in hits)
        {
            BaseMonster monster = hit.GetComponentInParent<BaseMonster>();
            if (monster == null || !hitMonsters.Add(monster))
                continue;

            Vector3 toMonster = monster.transform.position - origin;
            toMonster.y = 0f;
            if (toMonster.sqrMagnitude > CurrentAttackRange * CurrentAttackRange ||
                Vector3.Angle(direction, toMonster) > CurrentAttackAngle * 0.5f)
            {
                continue;
            }

            monster.TakeDamage(new DamageInfo
            {
                damage = Damage,
                attacker = gameObject,
                attackSource = gameObject,
                knockbackPower = weaponData.knockbackPower,
                knockbackDirection = toMonster.sqrMagnitude > 0.001f ? toMonster.normalized : direction
            });
        }
    }
}
