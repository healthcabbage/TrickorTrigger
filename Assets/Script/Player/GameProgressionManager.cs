using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        public BossMonster Spawn()
        {
            hasSpawned = true;
            if (boss == null)
            {
                Debug.LogError($"{eventName} 출현 일정에 BossMonster가 연결되지 않았습니다.");
                return null;
            }

            if (targetPumpkin == null)
                targetPumpkin = FindObjectOfType<Pumpkin>();

            if (targetPumpkin != null)
                boss.SetTarget(targetPumpkin);

            if (spawnPoint != null)
                boss.transform.position = spawnPoint.GetRandomSpawnPosition();

            boss.gameObject.SetActive(true);
            Debug.Log($"{eventName} 출현!");
            return boss;
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
    [Header("게임 종료")]
    [SerializeField] private Pumpkin targetPumpkin;

    private float gameplayElapsedSeconds;
    private bool auxiliaryWeaponsUnlocked;
    private bool gameOver;
    [Header("Canvas UI 참조")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject bossHudPanel;
    [SerializeField] private TextMeshProUGUI bossHudName;
    [SerializeField] private TextMeshProUGUI bossHudHealthText;
    [SerializeField] private Image bossHealthFill;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    private BossMonster currentHudBoss;
    private float displayedBossHealth = 1f;
    private float targetBossHealth = 1f;

    [SerializeField, Min(0.1f)] private float bossHealthBarLerpSpeed = 12f;

    public event Action AuxiliaryWeaponsUnlocked;
    public event Action GameOver;
    public float GameplayElapsedSeconds => gameplayElapsedSeconds;
    public bool IsGameOver => gameOver;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (targetPumpkin == null)
            targetPumpkin = FindObjectOfType<Pumpkin>();

        CreateBossHud();
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

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
        if (!gameOver && targetPumpkin != null && targetPumpkin.IsDead)
            HandleGameOver();

        if (gameOver)
            return;

        UpdateBossHud();

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
            {
                BossMonster spawnedBoss = schedule.Spawn();
                if (spawnedBoss != null)
                    ShowBossHud(spawnedBoss);
            }
        }
    }

    private void HandleGameOver()
    {
        if (gameOver)
            return;

        gameOver = true;
        Time.timeScale = 0f;
        CreateGameOverUI();
        GameOver?.Invoke();
        Debug.Log("게임 오버: 수호 호박이 파괴되었습니다.", this);
    }

    private void CreateGameOverUI()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    private void CreateBossHud()
    {
        if (bossHudPanel != null)
            bossHudPanel.SetActive(false);
    }

    private void ShowBossHud(BossMonster boss)
    {
        if (currentHudBoss != null)
            currentHudBoss.HealthChanged -= OnBossHealthChanged;

        currentHudBoss = boss;
        currentHudBoss.HealthChanged += OnBossHealthChanged;
        bossHudName.text = $"중간 보스 - {boss.BossDisplayName}";
        targetBossHealth = boss.HealthNormalized;
        displayedBossHealth = targetBossHealth;
        bossHealthFill.fillAmount = displayedBossHealth;
        UpdateBossHealthText();
        bossHudPanel.SetActive(true);
    }

    private void OnBossHealthChanged(float remainingHealth)
    {
        if (currentHudBoss != null)
            targetBossHealth = Mathf.Clamp01(
                remainingHealth / Mathf.Max(1f, currentHudBoss.MaxHealth));
    }

    private void UpdateBossHud()
    {
        if (currentHudBoss == null)
            return;

        if (currentHudBoss.IsDead)
        {
            currentHudBoss.HealthChanged -= OnBossHealthChanged;
            currentHudBoss = null;
            bossHudPanel.SetActive(false);
            return;
        }

        // Read the live value every frame as well as listening for damage events,
        // so the HUD cannot become stale if another damage path changes health.
        targetBossHealth = currentHudBoss.HealthNormalized;
        float lerpFactor = 1f - Mathf.Exp(-Mathf.Max(12f, bossHealthBarLerpSpeed) * Time.unscaledDeltaTime);
        displayedBossHealth = Mathf.Lerp(displayedBossHealth, targetBossHealth, lerpFactor);
        bossHealthFill.fillAmount = displayedBossHealth;
        UpdateBossHealthText();
    }

    private void UpdateBossHealthText()
    {
        if (currentHudBoss == null || bossHudHealthText == null)
            return;

        int shownHealth = Mathf.RoundToInt(currentHudBoss.CurrentHealth);
        int maxHealth = Mathf.RoundToInt(currentHudBoss.MaxHealth);
        bossHudHealthText.text = $"{shownHealth} / {maxHealth}";
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
