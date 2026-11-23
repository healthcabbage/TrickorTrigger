using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
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
    private GameObject gameOverCanvas;
    private GameObject bossHudCanvas;
    private GameObject bossHudPanel;
    private TextMeshProUGUI bossHudName;
    private TextMeshProUGUI bossHudHealthText;
    private Image bossHealthFill;
    private BossMonster currentHudBoss;
    private TMP_FontAsset uiFontAsset;
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
        if (gameOverCanvas != null)
            return;

        gameOverCanvas = new GameObject("Game Over UI");
        Canvas canvas = gameOverCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = gameOverCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameOverCanvas.AddComponent<GraphicRaycaster>();

        GameObject dimmer = CreateUiObject("Game Over Dimmer", gameOverCanvas.transform);
        RectTransform dimmerRect = dimmer.GetComponent<RectTransform>();
        dimmerRect.anchorMin = Vector2.zero;
        dimmerRect.anchorMax = Vector2.one;
        dimmerRect.offsetMin = Vector2.zero;
        dimmerRect.offsetMax = Vector2.zero;
        dimmer.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.68f);

        GameObject panelObject = CreateUiObject("Game Over Panel", dimmer.transform);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(560f, 340f);
        panelRect.anchoredPosition = Vector2.zero;
        panelObject.GetComponent<Image>().color = new Color(0.055f, 0.045f, 0.07f, 0.97f);

        CreateUiText("게임 오버", panelRect, new Vector2(0f, 92f), new Vector2(500f, 70f),
            48, new Color(1f, 0.43f, 0.22f));
        CreateUiText("호박이 파괴되었습니다.", panelRect,
            new Vector2(0f, 32f), new Vector2(500f, 42f), 22, Color.white);

        CreateUiButton("Restart", "다시 시작", panelRect, new Vector2(-112f, -78f), RestartGame,
            new Color(0.38f, 0.16f, 0.12f));
        CreateUiButton("Quit", "끝내기", panelRect, new Vector2(112f, -78f), QuitGame,
            new Color(0.2f, 0.19f, 0.23f));
    }

    private void CreateBossHud()
    {
        bossHudCanvas = new GameObject("Boss HUD");
        Canvas canvas = bossHudCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        canvas.sortingOrder = 20;
        CanvasScaler scaler = bossHudCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        bossHudCanvas.AddComponent<GraphicRaycaster>();

        bossHudPanel = CreateUiObject("Boss Health Panel", bossHudCanvas.transform);
        RectTransform panelRect = bossHudPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.sizeDelta = new Vector2(640f, 112f);
        panelRect.anchoredPosition = new Vector2(0f, -30f);
        bossHudPanel.GetComponent<Image>().color = new Color(0.045f, 0.035f, 0.05f, 0.88f);

        bossHudName = CreateUiText("중간 보스", panelRect, new Vector2(0f, -18f),
            new Vector2(600f, 36f), 26, new Color(1f, 0.82f, 0.55f));
        RectTransform nameRect = bossHudName.rectTransform;
        nameRect.anchorMin = nameRect.anchorMax = new Vector2(0.5f, 1f);
        nameRect.pivot = new Vector2(0.5f, 1f);

        GameObject healthBackground = CreateUiObject("Boss Health Background", panelRect);
        RectTransform backgroundRect = healthBackground.GetComponent<RectTransform>();
        backgroundRect.anchorMin = backgroundRect.anchorMax = new Vector2(0.5f, 0f);
        backgroundRect.pivot = new Vector2(0.5f, 0f);
        backgroundRect.sizeDelta = new Vector2(580f, 24f);
        backgroundRect.anchoredPosition = new Vector2(0f, 16f);
        healthBackground.GetComponent<Image>().color = new Color(0.15f, 0.11f, 0.12f, 1f);

        GameObject healthFill = CreateUiObject("Boss Health Fill", healthBackground.transform);
        RectTransform fillRect = healthFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);
        bossHealthFill = healthFill.GetComponent<Image>();
        bossHealthFill.color = new Color(0.72f, 0.12f, 0.13f, 1f);
        bossHealthFill.type = Image.Type.Filled;
        bossHealthFill.fillMethod = Image.FillMethod.Horizontal;
        bossHealthFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        bossHealthFill.fillAmount = 1f;

        bossHudHealthText = CreateUiText("500 / 500", healthBackground.transform,
            Vector2.zero, new Vector2(220f, 24f), 16, Color.white);

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

    private static GameObject CreateUiObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private TextMeshProUGUI CreateUiText(string value, Transform parent, Vector2 position,
        Vector2 size, int fontSize, Color color)
    {
        GameObject textObject = new GameObject(value, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = GetUiFontAsset(fontSize);
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private TMP_FontAsset GetUiFontAsset(int size)
    {
        if (uiFontAsset == null)
        {
            Font sourceFont = Font.CreateDynamicFontFromOSFont(
                new[]
                {
                    "Malgun Gothic", "맑은 고딕", "Noto Sans CJK KR",
                    "Noto Sans KR", "Gulim", "굴림", "Arial Unicode MS"
                },
                size);
            if (sourceFont == null)
                sourceFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (sourceFont != null)
            {
                uiFontAsset = TMP_FontAsset.CreateFontAsset(
                    sourceFont,
                    90,
                    9,
                    GlyphRenderMode.SDFAA,
                    1024,
                    1024,
                    AtlasPopulationMode.Dynamic,
                    true);
            }
        }

        return uiFontAsset;
    }

    private void CreateUiButton(string objectName, string label, Transform parent,
        Vector2 position, UnityEngine.Events.UnityAction onClick, Color color)
    {
        GameObject buttonObject = CreateUiObject(objectName, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(190f, 58f);
        rect.anchoredPosition = position;

        Image image = buttonObject.GetComponent<Image>();
        image.color = color;
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        CreateUiText(label, buttonObject.transform, Vector2.zero,
            new Vector2(180f, 52f), 22, Color.white);
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
