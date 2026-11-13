
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
}
