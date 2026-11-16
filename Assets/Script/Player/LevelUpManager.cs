using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerExperience playerExperience;
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject levelUpPanel;

    private Button[] optionButtons;
    private Text[] optionLabels;
    private Text panelTitle;
    private RectTransform panelBackground;
    private readonly Queue<int> pendingLevelUps = new Queue<int>();
    private bool hasChosenWeapon;
    private bool isChoosing;

    private void Awake()
    {
        if (playerExperience == null)
            playerExperience = FindObjectOfType<PlayerExperience>();
        if (weaponController == null)
            weaponController = FindObjectOfType<PlayerWeaponController>();
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        if (levelUpPanel != null)
        {
            optionButtons = levelUpPanel.GetComponentsInChildren<Button>(true);
            Array.Sort(optionButtons, (left, right) =>
                left.GetComponent<RectTransform>().anchoredPosition.x.CompareTo(
                    right.GetComponent<RectTransform>().anchoredPosition.x));
            optionLabels = new Text[optionButtons.Length];
            for (int i = 0; i < optionButtons.Length; i++)
                optionLabels[i] = EnsureButtonLabel(optionButtons[i]);

            SetupPanelLayout();

            levelUpPanel.SetActive(false);
        }

        if (weaponController != null)
            weaponController.SetGameplayActive(false);
        if (playerController != null)
            playerController.enabled = false;

        Time.timeScale = 0f;
    }

    private void OnEnable()
    {
        if (playerExperience != null)
            playerExperience.OnLevelUp += HandleLevelUp;
    }

    private void OnDisable()
    {
        if (playerExperience != null)
            playerExperience.OnLevelUp -= HandleLevelUp;
    }

    private void Start()
    {
        ShowStartingWeaponChoices();
    }

    private void ShowStartingWeaponChoices()
    {
        if (weaponController == null || optionButtons == null)
        {
            Debug.LogError("무기 선택 UI 또는 PlayerWeaponController가 연결되지 않았습니다.", this);
            return;
        }

        BaseWeapon[] weapons = weaponController.AvailableWeapons;
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogError("선택할 무기가 PlayerWeaponController에 등록되지 않았습니다.", this);
            return;
        }

        isChoosing = true;
        levelUpPanel.SetActive(true);
        int count = Mathf.Min(weapons.Length, optionButtons.Length);
        SetPanelTitle("무기를 선택하세요");
        LayoutOptionCards(count);
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i >= count || weapons[i] == null)
            {
                optionButtons[i].gameObject.SetActive(false);
                continue;
            }

            int weaponIndex = i;
            string label = $"{weapons[i].WeaponName}\n시작 무기로 선택";
            BindOption(i, label, () => SelectStartingWeapon(weaponIndex));
        }
    }

    private void SelectStartingWeapon(int weaponIndex)
    {
        if (weaponController == null || !weaponController.ChooseWeapon(weaponIndex))
            return;

        hasChosenWeapon = true;
        isChoosing = false;
        if (playerController != null)
            playerController.enabled = true;

        if (pendingLevelUps.Count > 0)
            ShowNextUpgradeChoice();
        else
            ContinueAfterChoice();
    }

    private void HandleLevelUp(int level)
    {
        pendingLevelUps.Enqueue(level);
        if (hasChosenWeapon && !isChoosing)
            ShowNextUpgradeChoice();
    }

    private void ShowNextUpgradeChoice()
    {
        if (!hasChosenWeapon || isChoosing)
            return;

        BaseWeapon weapon = weaponController != null ? weaponController.CurrentWeapon : null;
        while (pendingLevelUps.Count > 0 && (weapon == null || weapon.Level >= BaseWeapon.MaxLevel))
            pendingLevelUps.Dequeue();

        if (pendingLevelUps.Count == 0)
        {
            ContinueAfterChoice();
            return;
        }

        List<WeaponUpgradeType> upgrades = weapon.GetAvailableUpgrades();
        Shuffle(upgrades);
        if (upgrades.Count == 0)
        {
            pendingLevelUps.Dequeue();
            ShowNextUpgradeChoice();
            return;
        }

        isChoosing = true;
        Time.timeScale = 0f;
        weaponController.SetGameplayActive(false);
        if (playerController != null)
            playerController.enabled = false;
        levelUpPanel.SetActive(true);

        int count = Mathf.Min(upgrades.Count, optionButtons.Length);
        SetPanelTitle($"레벨 {pendingLevelUps.Peek()} · 강화 하나를 선택하세요");
        LayoutOptionCards(count);
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i >= count)
            {
                optionButtons[i].gameObject.SetActive(false);
                continue;
            }

            WeaponUpgradeType upgrade = upgrades[i];
            string label = $"{BaseWeapon.GetUpgradeLabel(upgrade)}\n{weapon.WeaponName} Lv.{weapon.Level} → Lv.{weapon.Level + 1}";
            BindOption(i, label, () => SelectUpgrade(upgrade));
        }
    }

    private void SelectUpgrade(WeaponUpgradeType upgrade)
    {
        BaseWeapon weapon = weaponController != null ? weaponController.CurrentWeapon : null;
        if (weapon != null && weapon.ApplyUpgrade(upgrade) && pendingLevelUps.Count > 0)
            pendingLevelUps.Dequeue();

        isChoosing = false;
        ShowNextUpgradeChoice();
    }

    private void BindOption(int index, string label, UnityEngine.Events.UnityAction callback)
    {
        Button button = optionButtons[index];
        button.gameObject.SetActive(true);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callback);

        if (optionLabels[index] != null)
        {
            RectTransform labelRect = optionLabels[index].rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = new Vector2(-32f, -32f);

            optionLabels[index].text = label;
            optionLabels[index].alignment = TextAnchor.MiddleCenter;
            optionLabels[index].fontSize = 24;
            optionLabels[index].resizeTextForBestFit = true;
            optionLabels[index].resizeTextMinSize = 18;
            optionLabels[index].resizeTextMaxSize = 26;
            optionLabels[index].color = Color.white;
            optionLabels[index].raycastTarget = false;
        }
    }

    private void SetupPanelLayout()
    {
        Image backgroundImage = levelUpPanel.GetComponentInChildren<Image>(true);
        if (backgroundImage != null)
        {
            panelBackground = backgroundImage.rectTransform;
            panelBackground.anchorMin = new Vector2(0.5f, 0.5f);
            panelBackground.anchorMax = new Vector2(0.5f, 0.5f);
            panelBackground.pivot = new Vector2(0.5f, 0.5f);
            panelBackground.anchoredPosition = Vector2.zero;
            panelBackground.sizeDelta = new Vector2(1020f, 560f);
            backgroundImage.color = new Color(0.07f, 0.09f, 0.14f, 0.97f);
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            ColorBlock colors = optionButtons[i].colors;
            colors.normalColor = new Color(0.16f, 0.2f, 0.29f, 1f);
            colors.highlightedColor = new Color(0.27f, 0.38f, 0.53f, 1f);
            colors.pressedColor = new Color(0.1f, 0.15f, 0.23f, 1f);
            colors.selectedColor = colors.highlightedColor;
            optionButtons[i].colors = colors;

            Image cardImage = optionButtons[i].targetGraphic as Image;
            if (cardImage != null)
                cardImage.color = Color.white;
        }

        if (optionLabels.Length > 0 && optionLabels[0] != null && panelBackground != null)
        {
            GameObject titleObject = Instantiate(optionLabels[0].gameObject, panelBackground, false);
            titleObject.name = "Choice Panel Title";
            panelTitle = titleObject.GetComponent<Text>();
            RectTransform titleRect = titleObject.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -22f);
            titleRect.sizeDelta = new Vector2(920f, 72f);
            if (panelTitle != null)
            {
                panelTitle.raycastTarget = false;
                panelTitle.alignment = TextAnchor.MiddleCenter;
                panelTitle.fontSize = 36;
                panelTitle.color = Color.white;
                panelTitle.resizeTextForBestFit = true;
                panelTitle.resizeTextMinSize = 24;
                panelTitle.resizeTextMaxSize = 36;
            }
        }
    }

    private void LayoutOptionCards(int count)
    {
        if (count <= 0)
            return;

        const float cardWidth = 285f;
        const float cardHeight = 320f;
        const float spacing = 310f;
        float firstX = -spacing * (count - 1) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            RectTransform cardRect = optionButtons[i].GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = new Vector2(firstX + spacing * i, -58f);
            cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);
        }
    }

    private void SetPanelTitle(string title)
    {
        if (panelTitle != null)
            panelTitle.text = title;
    }

    private static Text EnsureButtonLabel(Button button)
    {
        TMP_Text existingTmpLabel = button.GetComponentInChildren<TMP_Text>(true);
        if (existingTmpLabel != null)
            existingTmpLabel.enabled = false;

        Text label = button.GetComponentInChildren<Text>(true);
        if (label == null)
        {
            GameObject labelObject = new GameObject(
                "Choice Label",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            labelObject.transform.SetParent(button.transform, false);
            label = labelObject.GetComponent<Text>();
        }

        RectTransform rect = label.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(16f, 16f);
        rect.offsetMax = new Vector2(-16f, -16f);
        label.transform.SetAsLastSibling();

        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = false;
        return label;
    }

    private void ContinueAfterChoice()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        isChoosing = false;
        if (hasChosenWeapon)
        {
            if (playerController != null)
                playerController.enabled = true;
            if (weaponController != null)
                weaponController.SetGameplayActive(true);
            Time.timeScale = 1f;
        }
    }

    private static void Shuffle(List<WeaponUpgradeType> values)
    {
        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            WeaponUpgradeType temp = values[i];
            values[i] = values[j];
            values[j] = temp;
        }
    }
}
