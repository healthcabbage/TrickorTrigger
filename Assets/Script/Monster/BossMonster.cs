using UnityEngine;

public class BossMonster : BaseMonster
{
    private enum BossAction
    {
        Approach,
        NormalAttackWindup,
        ChargeWindup,
        Charge,
        ShockwaveWindup,
        Recovery
    }

    [Header("Boss Data")]
    [SerializeField] private BossStats bossStats;

    [Header("Special Attack Target")]
    [SerializeField] private PlayerController targetPlayer;

    private BossAction action;
    private float actionTimer;
    private float normalAttackTimer;
    private float specialAttackTimer;
    private float chargeTravelled;
    private Vector3 chargeDirection;
    private bool chargeHitPlayer;

    private LineRenderer normalTelegraph;
    private LineRenderer chargeTelegraph;
    private LineRenderer shockwaveTelegraph;
    private Material telegraphMaterial;

    public override void InitializeMonster()
    {
        if (bossStats == null)
        {
            Debug.LogError("Boss Stats 에셋이 연결되지 않았습니다.", this);
            isInitialized = false;
            return;
        }

        if (!string.IsNullOrWhiteSpace(bossStats.bossName))
            gameObject.name = bossStats.bossName;

        _currentHealth = bossStats.maxHealth;
        isDead = false;
        isInitialized = true;
    }

    public override void SetTarget(Pumpkin pumpkin)
    {
        if (pumpkin == null)
        {
            Debug.LogError("Target Pumpkin이 연결되지 않았습니다.", this);
            return;
        }

        targetPumpkin = pumpkin;
    }

    private void Start()
    {
        CreateTelegraphs();
    }

    protected override void Update()
    {
        if (!isInitialized || isDead)
            return;

        if (targetPlayer == null)
            targetPlayer = FindObjectOfType<PlayerController>();

        normalAttackTimer += Time.deltaTime;
        if (targetPlayer != null)
            specialAttackTimer += Time.deltaTime;

        switch (action)
        {
            case BossAction.Approach:
                UpdateApproach();
                break;
            case BossAction.NormalAttackWindup:
                UpdateNormalAttackWindup();
                break;
            case BossAction.ChargeWindup:
                UpdateChargeWindup();
                break;
            case BossAction.Charge:
                UpdateCharge();
                break;
            case BossAction.ShockwaveWindup:
                UpdateShockwaveWindup();
                break;
            case BossAction.Recovery:
                UpdateRecovery();
                break;
        }

        UpdateTelegraphs();
    }

    private void UpdateApproach()
    {
        if (targetPlayer != null && specialAttackTimer >= bossStats.specialAttackCooldown)
        {
            BeginChargeWindup(Flatten(targetPlayer.transform.position - transform.position));
            return;
        }

        if (targetPumpkin == null)
            return;

        Vector3 toPumpkin = Flatten(targetPumpkin.transform.position - transform.position);
        float distance = toPumpkin.magnitude;
        if (distance <= bossStats.normalAttackRange)
        {
            if (normalAttackTimer >= bossStats.normalAttackCooldown)
                BeginNormalAttackWindup(toPumpkin);
            return;
        }

        if (distance > 0.001f)
        {
            Face(toPumpkin);
            transform.position += toPumpkin.normalized * bossStats.moveSpeed * Time.deltaTime;
        }
    }

    private void BeginNormalAttackWindup(Vector3 direction)
    {
        Face(direction);
        action = BossAction.NormalAttackWindup;
        actionTimer = 0f;
        normalAttackTimer = 0f;
    }

    private void UpdateNormalAttackWindup()
    {
        actionTimer += Time.deltaTime;
        if (actionTimer < bossStats.normalAttackWindup)
            return;

        DealConeDamage();
        BeginRecovery(0.35f);
    }

    private void DealConeDamage()
    {
        if (targetPumpkin == null)
            return;

        Vector3 toPumpkin = Flatten(targetPumpkin.transform.position - transform.position);
        if (toPumpkin.magnitude <= bossStats.normalAttackRange &&
            Vector3.Angle(transform.forward, toPumpkin) <= bossStats.normalAttackAngle * 0.5f)
        {
            targetPumpkin.TakeDamage(bossStats.normalAttackDamage);
        }
    }

    private void BeginChargeWindup(Vector3 toPlayer)
    {
        if (toPlayer.sqrMagnitude < 0.001f)
            toPlayer = transform.forward;

        chargeDirection = toPlayer.normalized;
        Face(chargeDirection);
        action = BossAction.ChargeWindup;
        actionTimer = 0f;
        specialAttackTimer = 0f;
        chargeTravelled = 0f;
        chargeHitPlayer = false;
    }

    private void UpdateChargeWindup()
    {
        actionTimer += Time.deltaTime;
        if (actionTimer < bossStats.chargeWindup)
            return;

        action = BossAction.Charge;
        actionTimer = 0f;
    }

    private void UpdateCharge()
    {
        if (targetPlayer == null)
        {
            BeginShockwaveWindup();
            return;
        }

        float step = bossStats.chargeSpeed * Time.deltaTime;
        step = Mathf.Min(step, Mathf.Max(0f, bossStats.chargeDistance - chargeTravelled));
        Vector3 from = transform.position;
        Vector3 to = from + chargeDirection * step;

        if (!chargeHitPlayer && DistanceToSegment(targetPlayer.transform.position, from, to) <= bossStats.chargeHitRadius)
        {
            targetPlayer.TakeDamage(bossStats.chargeDamage);
            chargeHitPlayer = true;
        }

        transform.position = to;
        chargeTravelled += step;
        if (chargeTravelled >= bossStats.chargeDistance)
            BeginShockwaveWindup();
    }

    private void BeginShockwaveWindup()
    {
        action = BossAction.ShockwaveWindup;
        actionTimer = 0f;
    }

    private void UpdateShockwaveWindup()
    {
        actionTimer += Time.deltaTime;
        if (actionTimer < bossStats.shockwaveWindup)
            return;

        if (targetPlayer != null &&
            Vector3.Distance(transform.position, targetPlayer.transform.position) <= bossStats.shockwaveRadius)
        {
            targetPlayer.TakeDamage(bossStats.shockwaveDamage);
        }

        BeginRecovery(bossStats.recoveryDuration);
    }

    private void BeginRecovery(float duration)
    {
        action = BossAction.Recovery;
        actionTimer = -duration;
    }

    private void UpdateRecovery()
    {
        actionTimer += Time.deltaTime;
        if (actionTimer >= 0f)
            action = BossAction.Approach;
    }

    private void CreateTelegraphs()
    {
        if (bossStats == null)
            return;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");
        if (shader == null)
        {
            Debug.LogWarning("공격 예고 표시용 셰이더를 찾지 못했습니다.", this);
            return;
        }

        telegraphMaterial = new Material(shader);
        normalTelegraph = CreateTelegraphLine("Normal Attack Warning", new Color(1f, 0.55f, 0.05f, 0.9f));
        chargeTelegraph = CreateTelegraphLine("Charge Warning", new Color(1f, 0.1f, 0.1f, 0.9f));
        shockwaveTelegraph = CreateTelegraphLine("Shockwave Warning", new Color(0.1f, 0.75f, 1f, 0.9f));
        SetTelegraphsVisible(false, false, false);
    }

    private LineRenderer CreateTelegraphLine(string objectName, Color color)
    {
        GameObject lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = false;
        line.widthMultiplier = 0.07f;
        line.startColor = color;
        line.endColor = color;
        line.sharedMaterial = telegraphMaterial;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.alignment = LineAlignment.View;
        line.enabled = false;
        return line;
    }

    private void UpdateTelegraphs()
    {
        if (normalTelegraph == null)
            return;

        bool showNormal = action == BossAction.NormalAttackWindup;
        bool showCharge = action == BossAction.ChargeWindup;
        bool showShockwave = action == BossAction.ShockwaveWindup;
        SetTelegraphsVisible(showNormal, showCharge, showShockwave);

        Vector3 center = transform.position + Vector3.up * 0.08f;
        if (showNormal)
            UpdateConeTelegraph(center);
        if (showCharge)
            UpdateChargeTelegraph(center);
        if (showShockwave)
            SetLinePoints(shockwaveTelegraph,
                CreateCirclePoints(center, bossStats.shockwaveRadius, 48));
    }

    private void UpdateConeTelegraph(Vector3 center)
    {
        const int segments = 32;
        Vector3[] points = new Vector3[segments + 3];
        points[0] = center;
        float halfAngle = bossStats.normalAttackAngle * 0.5f;
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);
            points[i + 1] = center + Quaternion.Euler(0f, angle, 0f)
                * transform.forward * bossStats.normalAttackRange;
        }
        points[points.Length - 1] = center;
        SetLinePoints(normalTelegraph, points);
    }

    private void UpdateChargeTelegraph(Vector3 center)
    {
        Vector3 right = Vector3.Cross(Vector3.up, chargeDirection).normalized;
        float halfWidth = bossStats.chargeHitRadius;
        Vector3 end = center + chargeDirection * bossStats.chargeDistance;
        Vector3[] points =
        {
            center - right * halfWidth,
            center + right * halfWidth,
            end + right * halfWidth,
            end - right * halfWidth,
            center - right * halfWidth
        };
        SetLinePoints(chargeTelegraph, points);
    }

    private void SetTelegraphsVisible(bool showNormal, bool showCharge, bool showShockwave)
    {
        normalTelegraph.enabled = showNormal;
        chargeTelegraph.enabled = showCharge;
        shockwaveTelegraph.enabled = showShockwave;
    }

    private static Vector3[] CreateCirclePoints(Vector3 center, float radius, int segments)
    {
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            points[i] = center + new Vector3(Mathf.Sin(angle) * radius, 0f,
                Mathf.Cos(angle) * radius);
        }
        return points;
    }

    private static void SetLinePoints(LineRenderer line, Vector3[] points)
    {
        line.positionCount = points.Length;
        line.SetPositions(points);
    }

    private void Face(Vector3 direction)
    {
        direction = Flatten(direction);
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    private static Vector3 Flatten(Vector3 vector)
    {
        vector.y = 0f;
        return vector;
    }

    private static float DistanceToSegment(Vector3 point, Vector3 start, Vector3 end)
    {
        Vector3 segment = end - start;
        float lengthSquared = segment.sqrMagnitude;
        if (lengthSquared < 0.0001f)
            return Vector3.Distance(point, start);

        float t = Mathf.Clamp01(Vector3.Dot(point - start, segment) / lengthSquared);
        return Vector3.Distance(point, start + t * segment);
    }

    private void OnDestroy()
    {
        if (telegraphMaterial != null)
            Destroy(telegraphMaterial);
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        action = BossAction.Approach;
        actionTimer = 0f;
        normalAttackTimer = bossStats != null ? bossStats.normalAttackCooldown : 0f;
        specialAttackTimer = 0f;
        chargeTravelled = 0f;
        chargeHitPlayer = false;
    }
}
