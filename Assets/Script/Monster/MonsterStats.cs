using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStats", menuName = "Scriptable Objects/MonsterStats")]
public class MonsterStats : ScriptableObject
{
    [Header("Monster")]
    public string monsterName;

    [Header("Combat")]
    public float maxHealth = 0;
    public float attackDamage = 0;
    public float movespeed = 0;
    public float attackRange = 0;
    public float attackInterval = 0f;
}
