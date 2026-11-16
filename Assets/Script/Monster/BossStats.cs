using UnityEngine;

public enum BossType
{
    MiniBoss,
    FinalBoss
}

[CreateAssetMenu(fileName = "BossStats", menuName = "Scriptable Objects/Boss Stats")]
public class BossStats : ScriptableObject
{
    [Header("Boss")]
    public BossType bossType;
    public string bossName;
    [Min(0f)] public float maxHealth = 0f;
    [Min(0f)] public float moveSpeed = 0f;

    [Header("Normal Attack - Front Cone")]
    [Min(0f)] public float normalAttackDamage = 0f;
    [Min(0f)] public float normalAttackRange = 0f;
    [Range(0f, 180f)] public float normalAttackAngle = 0f;
    [Min(0f)] public float normalAttackCooldown = 0f;
    [Min(0f)] public float normalAttackWindup = 0f;

    [Header("Special Attack - Charge")]
    [Min(0f)] public float specialAttackCooldown = 0f;
    [Min(0f)] public float chargeWindup = 0f;
    [Min(0f)] public float chargeSpeed = 0f;
    [Min(0f)] public float chargeDistance = 0f;
    [Min(0f)] public float chargeHitRadius = 0f;
    [Min(0f)] public float chargeDamage = 0f;

    [Header("Special Attack - Scythe Slam Shockwave")]
    [Min(0f)] public float shockwaveWindup = 0f;
    [Min(0f)] public float shockwaveRadius = 0f;
    [Min(0f)] public float shockwaveDamage = 0f;
    [Min(0f)] public float recoveryDuration = 0f;
}
