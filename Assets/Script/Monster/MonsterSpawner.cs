
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Monster")]
    [SerializeField]
    private BaseMonster monsterPrefab;

    [Header("Target")]
    [SerializeField]
    private Transform targetPumpkin;

    [Header("Spawn")]
    [SerializeField]
    private SpawnPoint spawnPoint;

    [SerializeField]
    private float spawnInterval = 2f;

    [SerializeField]
    private int maxMonsterCount = 30;

    private float spawnTimer;

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval)
            return;

        spawnTimer = 0f;

        SpawnMonster();
    }

    private void SpawnMonster()
    {
        if (monsterPrefab == null)
            return;

        if (spawnPoint == null)
            return;

        // 활성 몬스터 수 제한
        if (GetActiveMonsterCount() >= maxMonsterCount)
            return;

        Vector3 spawnPosition =
            spawnPoint.GetRandomSpawnPosition();

        BaseMonster monster = Instantiate(
            monsterPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }

    private int GetActiveMonsterCount()
    {
        return FindObjectsByType<BaseMonster>(
            FindObjectsSortMode.None
        ).Length;
    }
}
