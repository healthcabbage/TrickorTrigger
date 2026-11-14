
using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] protected WeaponData weaponData;

    protected float attackTimer;

    public float AttackInterval =>
    weaponData != null ? weaponData.attackInterval : 0.5f;

    public virtual void Initialize(WeaponData data)
    {
        weaponData = data;
        attackTimer = 0f;
    }

    public abstract void Attack(Vector3 direction);
}
