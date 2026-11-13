using UnityEngine;

public struct DamageInfo
{
    public float damage;

    // 공격한 대상
    public GameObject attacker;

    // 공격에 사용된 무기 또는 스킬
    public GameObject attackSource;

    // 치명타 여부
    public bool isCritical;

    // 넉백 방향
    public Vector3 knockbackDirection;

    // 넉백 강도
    public float knockbackPower;
}
