
using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponData",
    menuName = "Game/Weapon Data"
)]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;

    [Header("Attack")]
    public float damage = 10f;
    public float attackInterval = 0.5f;
    public float knockbackPower = 0f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;
    public float projectileLifetime = 3f;

    [Header("Melee Cone")]
    [Tooltip("근접 부채꼴 공격의 최대 거리입니다.")]
    [Min(0f)] public float attackRange = 2f;
    [Tooltip("근접 부채꼴 공격의 전체 각도입니다.")]
    [Range(0f, 180f)] public float attackAngle = 90f;

    [Header("Bomb")]
    [Tooltip("폭탄이 이동할 최대 사거리입니다.")]
    [Min(0f)] public float throwRange = 8f;
    [Tooltip("폭탄이 사거리에 도달한 뒤 폭발하기까지의 대기 시간(초)입니다.")]
    [Min(0f)] public float explosionDelay = 1f;
    [Tooltip("폭발로 피해를 주는 반경입니다.")]
    [Min(0f)] public float explosionRadius = 3f;
}
