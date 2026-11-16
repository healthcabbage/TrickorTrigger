using System;
using UnityEngine;

public class GameProgressionManager : MonoBehaviour
{
    [Serializable]
    private class BossSpawnSchedule
    {
        [SerializeField] private string eventName = "중간 보스";
        [SerializeField, Min(0f), Tooltip("게임 진행 시간 기준 출현 시점(초)입니다.")]
        private float spawnAtSeconds = 180f;
        [SerializeField] private BossMonster boss;
        [SerializeField] private SpawnPoint spawnPoint;
        [SerializeField] private Pumpkin targetPumpkin;

        [NonSerialized] private bool hasSpawned;

        public bool IsDue(float elapsedSeconds) => !hasSpawned && elapsedSeconds >= spawnAtSeconds;

        public void Spawn()
        {
            hasSpawned = true;
            if (boss == null)
            {
                Debug.LogError($"{eventName} 출현 일정에 BossMonster가 연결되지 않았습니다.");
                return;
            }

            if (targetPumpkin == null)
                targetPumpkin = FindObjectOfType<Pumpkin>();

            if (targetPumpkin != null)
                boss.SetTarget(targetPumpkin);

            if (spawnPoint != null)
                boss.transform.position = spawnPoint.GetRandomSpawnPosition();

            boss.gameObject.SetActive(true);
            Debug.Log($"{eventName} 출현!");
        }

        public void KeepInactive()
        {
            if (boss != null)
                boss.gameObject.SetActive(false);
        }
    }

    [Header("진행 시간 (초)")]
    [SerializeField, Min(0f), Tooltip("주 무기로 플레이를 시작한 뒤 보조 무기 선택이 해금될 때까지의 시간(초)입니다.")]
    private float auxiliaryWeaponUnlockTimeSeconds = 60f;
    [SerializeField, Tooltip("보스별 출현 시각과 등장 위치를 등록합니다. 최종 보스 일정도 여기에 추가할 수 있습니다.")]
    private BossSpawnSchedule[] bossSpawnSchedules;

    private float gameplayElapsedSeconds;
    private bool auxiliaryWeaponsUnlocked;

    public event Action AuxiliaryWeaponsUnlocked;
    public float GameplayElapsedSeconds => gameplayElapsedSeconds;

    private void Awake()
    {
        if (bossSpawnSchedules == null)
            return;

        foreach (BossSpawnSchedule schedule in bossSpawnSchedules)
        {
            if (schedule != null)
                schedule.KeepInactive();
        }
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        gameplayElapsedSeconds += Time.deltaTime;

        if (!auxiliaryWeaponsUnlocked && gameplayElapsedSeconds >= auxiliaryWeaponUnlockTimeSeconds)
        {
            auxiliaryWeaponsUnlocked = true;
            AuxiliaryWeaponsUnlocked?.Invoke();
        }

        if (bossSpawnSchedules == null)
            return;

        foreach (BossSpawnSchedule schedule in bossSpawnSchedules)
        {
            if (schedule != null && schedule.IsDue(gameplayElapsedSeconds))
                schedule.Spawn();
        }
    }
}
