using UnityEngine;

public class ProtectiveBarrierAuxiliaryWeapon : AuxiliaryWeapon
{
    private float timer;
    private LineRenderer[] shieldRings;
    private Material ringMaterial;

    protected override string DefaultDisplayName => "수호 방호벽";
    protected override string DefaultDescription => "일정 시간마다 플레이어를 모든 피해로부터 보호합니다.";

    public override System.Collections.Generic.List<AuxiliaryUpgradeType> GetAvailableUpgrades()
    {
        if (Level >= MaxLevel)
            return new System.Collections.Generic.List<AuxiliaryUpgradeType>();
        return new System.Collections.Generic.List<AuxiliaryUpgradeType>
        {
            AuxiliaryUpgradeType.Duration,
            AuxiliaryUpgradeType.Cooldown,
            AuxiliaryUpgradeType.Recovery
        };
    }

    public override void SetEquipped(bool equipped)
    {
        base.SetEquipped(equipped);
        if (equipped && shieldRings == null)
            CreateShieldVisual();

        UpdateShieldVisual();
    }

    public override void Tick(float deltaTime)
    {
        if (player == null)
            return;

        timer += deltaTime;
        if (timer >= CurrentInterval(stats.attackInterval))
        {
            timer = 0f;
            player.GrantShield(CurrentDuration(stats.duration));
            if (RecoveryBonus > 0f)
                player.Heal(player.MaxHealth * RecoveryBonus);
        }

        UpdateShieldVisual();
    }

    private void CreateShieldVisual()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");
        if (shader == null)
            return;

        ringMaterial = new Material(shader);
        GameObject visual = new GameObject("Player Protective Barrier");
        visual.transform.SetParent(owner, false);
        shieldRings = new LineRenderer[3];
        for (int i = 0; i < shieldRings.Length; i++)
        {
            GameObject ringObject = new GameObject($"Barrier Ring {i + 1}");
            ringObject.transform.SetParent(visual.transform, false);
            LineRenderer ring = ringObject.AddComponent<LineRenderer>();
            ring.useWorldSpace = true;
            ring.loop = true;
            ring.positionCount = 40;
            ring.widthMultiplier = 0.065f;
            ring.startColor = new Color(0.25f, 0.85f, 1f, 0.9f);
            ring.endColor = ring.startColor;
            ring.sharedMaterial = ringMaterial;
            ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ring.receiveShadows = false;
            ring.alignment = LineAlignment.View;
            shieldRings[i] = ring;
        }
    }

    private void UpdateShieldVisual()
    {
        if (shieldRings == null || owner == null || player == null)
            return;

        bool visible = isEquipped && player.IsShielded;
        Vector3 center = owner.position + Vector3.up * 0.9f;
        float visualRadius = CurrentRange(stats.range);
        for (int ringIndex = 0; ringIndex < shieldRings.Length; ringIndex++)
        {
            LineRenderer ring = shieldRings[ringIndex];
            ring.enabled = visible;
            if (!visible)
                continue;

            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = 2f * Mathf.PI * i / ring.positionCount;
                Vector3 point;
                if (ringIndex == 0)
                    point = center + new Vector3(Mathf.Cos(angle) * visualRadius, 0f, Mathf.Sin(angle) * visualRadius);
                else if (ringIndex == 1)
                    point = center + new Vector3(Mathf.Cos(angle) * visualRadius, Mathf.Sin(angle) * visualRadius, 0f);
                else
                    point = center + new Vector3(0f, Mathf.Sin(angle) * visualRadius, Mathf.Cos(angle) * visualRadius);
                ring.SetPosition(i, point);
            }
        }
    }

    private void OnDestroy()
    {
        if (ringMaterial != null)
            Destroy(ringMaterial);
    }
}
