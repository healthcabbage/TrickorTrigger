using UnityEngine;

[CreateAssetMenu(fileName = "PumpkinState", menuName = "Scriptable Objects/PumpkinState")]
public class PumpkinState : ScriptableObject
{
    [Header("생존")]
    public float maxHealth = 0f;
    public float defense = 0;

    [Header("자가회복")]
    public float recovery = 0f;

    //호박이 주변에 자동 공격 기능은 생각해보기
}
