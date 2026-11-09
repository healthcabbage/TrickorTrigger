using UnityEngine;

[CreateAssetMenu(fileName = "MonsterReward", menuName = "Scriptable Objects/MonsterReward")]
public class MonsterReward : ScriptableObject
{
    [Header("Coin")]
    public GameObject coinPrefab;
    public int coindropcount = 0;
    public int coinamountdrop = 0;

    [Header("Candy")]
    public GameObject candyPrefab;
    public int candydropcount = 0;
    public int candyamountdrop = 0;
}
