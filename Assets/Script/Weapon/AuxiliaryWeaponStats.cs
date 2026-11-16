using UnityEngine;

[CreateAssetMenu(fileName = "AuxiliaryWeaponStats", menuName = "Game/Auxiliary Weapon Stats")]
public class AuxiliaryWeaponStats : ScriptableObject
{
    [Header("표시")]
    public string weaponName;
    [TextArea] public string description;
    [Min(1)] public int maxLevel = 7;

    [Header("기본 전투 수치")]
    [Tooltip("한 번의 타격 또는 투사체가 주는 피해입니다.")][Min(0f)] public float damage = 8f;
    [Tooltip("공격 또는 효과 발동 사이의 시간(초)입니다.")][Min(0.05f)] public float attackInterval = 1f;
    [Tooltip("공격 반경, 충격파 반경 또는 방호벽 시각 반경입니다.")][Min(0f)] public float range = 1f;
    [Tooltip("투사체 속도 또는 궤도 회전 속도입니다.")][Min(0f)] public float speed = 1f;
    [Tooltip("투사체 비행 시간, 파동 확산 시간 또는 방호벽 지속 시간입니다.")][Min(0f)] public float duration = 1f;
    [Tooltip("충격파가 적을 밀쳐내는 거리입니다.")][Min(0f)] public float knockbackPower = 1f;
    [Tooltip("플레이어 또는 호박 중심으로 도는 궤도의 반경입니다.")][Min(0f)] public float orbitRadius = 1f;
    [Tooltip("플레이어 또는 호박을 도는 각속도(도/초)입니다.")][Min(0f)] public float orbitSpeed = 90f;
    [Tooltip("방호벽 회복 강화 한 번당 최대 체력에 더하는 비율입니다.")][Min(0f)] public float recoveryPerUpgrade = 0.02f;

    [Header("레벨당 강화 배율")]
    [Min(1f)] public float damageUpgradeMultiplier = 1.2f;
    [Min(1f)] public float rangeUpgradeMultiplier = 1.15f;
    [Range(0.1f, 1f)] public float intervalUpgradeMultiplier = 0.9f;
    [Min(1f)] public float speedUpgradeMultiplier = 1.15f;
    [Min(1f)] public float durationUpgradeMultiplier = 1.2f;
    [Min(1f)] public float knockbackUpgradeMultiplier = 1.2f;
}
