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
    [Tooltip("보스 종류를 구분합니다. 중간 보스 또는 최종 보스를 선택합니다.")]
    public BossType bossType;
    [Tooltip("보스 이름입니다. 보스 오브젝트 이름에도 적용됩니다.")]
    public string bossName;
    [Tooltip("보스의 최대 체력입니다.")]
    [Min(0f)] public float maxHealth = 0f;
    [Tooltip("호박을 향해 이동할 때의 속도입니다.")]
    [Min(0f)] public float moveSpeed = 0f;

    [Header("Normal Attack - Front Cone")]
    [Tooltip("부채꼴 일반 공격이 호박에게 주는 피해량입니다.")]
    [Min(0f)] public float normalAttackDamage = 0f;
    [Tooltip("부채꼴 일반 공격의 최대 거리입니다.")]
    [Min(0f)] public float normalAttackRange = 0f;
    [Tooltip("부채꼴 일반 공격의 전체 각도입니다. 좌우로 절반씩 적용됩니다.")]
    [Range(0f, 180f)] public float normalAttackAngle = 0f;
    [Tooltip("일반 공격을 다시 사용할 수 있을 때까지의 대기 시간(초)입니다.")]
    [Min(0f)] public float normalAttackCooldown = 0f;
    [Tooltip("일반 공격 게이지가 차오른 뒤 실제 공격까지의 예고 시간(초)입니다.")]
    [Min(0f)] public float normalAttackWindup = 0f;

    [Header("Skill Schedule")]
    [Tooltip("몇 번의 일반 공격 후 특수 공격을 시도할지 정합니다.")]
    [Min(1)] public int normalAttacksBeforeSpecial = 3;

    [Header("Special Attack - Charge")]
    [Tooltip("돌진 스킬을 다시 사용할 수 있을 때까지의 대기 시간(초)입니다.")]
    [Min(0f)] public float chargeCooldown = 0f;
    [Tooltip("돌진 게이지를 채우며 플레이어에게 돌진을 예고하는 시간(초)입니다.")]
    [Min(0f)] public float chargeWindup = 0f;
    [Tooltip("돌진하는 속도입니다.")]
    [Min(0f)] public float chargeSpeed = 0f;
    [Tooltip("돌진이 이동할 최대 거리입니다.")]
    [Min(0f)] public float chargeDistance = 0f;
    [Tooltip("돌진 경로의 반폭입니다. 이 범위에 닿은 플레이어가 피해를 받습니다.")]
    [Min(0f)] public float chargeHitRadius = 0f;
    [Tooltip("돌진 경로에 맞은 플레이어가 받는 피해량입니다.")]
    [Min(0f)] public float chargeDamage = 0f;

    [Header("Special Attack - Scythe Slam Shockwave")]
    [Tooltip("충격파 스킬을 다시 사용할 수 있을 때까지의 대기 시간(초)입니다.")]
    [Min(0f)] public float shockwaveCooldown = 0f;
    [Tooltip("충격파 게이지를 채우며 낫 내려치기를 예고하는 시간(초)입니다.")]
    [Min(0f)] public float shockwaveWindup = 0f;
    [Tooltip("낫 내려치기 충격파가 닿는 반경입니다.")]
    [Min(0f)] public float shockwaveRadius = 0f;
    [Tooltip("충격파에 맞은 플레이어가 받는 피해량입니다.")]
    [Min(0f)] public float shockwaveDamage = 0f;
    [Tooltip("공격이 끝난 뒤 다음 행동을 시작하기 전의 회복 시간(초)입니다.")]
    [Min(0f)] public float recoveryDuration = 0f;
}
