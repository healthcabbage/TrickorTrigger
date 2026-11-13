using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Monster")]
    [SerializeField] private BaseMonster[] monsterPrefabs;

    [Header("Target")]
    [SerializeField] private Pumpkin targetPumpkin;

    [Header("Spawn")]
    [SerializeField] private SpawnPoint spawnPoint;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxMonsterCount = 30;

    private float spawnTimer;

    // 이 Spawner가 생성한 몬스터 목록
    private readonly List<BaseMonster> activeMonsters = new();

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval)
            return;

        spawnTimer -= spawnInterval;

        SpawnMonster();
    }

    private void SpawnMonster()
    {
        // 필수 참조 확인
        if (monsterPrefabs == null || monsterPrefabs.Length == 0)
            return;

        if (targetPumpkin == null || spawnPoint == null)
            return;

        // 이미 파괴된 몬스터 제거
        activeMonsters.RemoveAll(monster => monster == null);

        // 활성 몬스터 수 제한
        if (activeMonsters.Count >= maxMonsterCount)
            return;

        // 랜덤 몬스터 선택
        int randomIndex = Random.Range(0, monsterPrefabs.Length);
        BaseMonster prefab = monsterPrefabs[randomIndex];

        if (prefab == null)
            return;

        Vector3 spawnPosition = spawnPoint.GetRandomSpawnPosition();

        BaseMonster monster = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity
        );

        // 목표 설정
        monster.SetTarget(targetPumpkin);

        activeMonsters.Add(monster);
    }
}