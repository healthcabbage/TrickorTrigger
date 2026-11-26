using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpManager : MonoBehaviour
{
    private struct UpgradeChoice
    {
        public BaseWeapon primaryWeapon;
        public WeaponUpgradeType primaryUpgrade;
        public AuxiliaryWeapon auxiliaryWeapon;
        public AuxiliaryUpgradeType auxiliaryUpgrade;
        public AuxiliaryWeapon newAuxiliaryWeapon;
        public int newAuxiliaryIndex;

        public static UpgradeChoice Primary(BaseWeapon weapon, WeaponUpgradeType upgrade)
        {
            return new UpgradeChoice { primaryWeapon = weapon, primaryUpgrade = upgrade };
        }

        public static UpgradeChoice Auxiliary(AuxiliaryWeapon weapon, AuxiliaryUpgradeType upgrade)
        {
            return new UpgradeChoice { auxiliaryWeapon = weapon, auxiliaryUpgrade = upgrade };
        }

        public static UpgradeChoice NewAuxiliary(AuxiliaryWeapon weapon, int index)
        {
            return new UpgradeChoice { newAuxiliaryWeapon = weapon, newAuxiliaryIndex = index };
        }
    }

    [Header("References")]
    [SerializeField] private PlayerExperience playerExperience;
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private PlayerAuxiliaryWeaponController auxiliaryWeaponController;
    [SerializeField] private GameProgressionManager progressionManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TMP_FontAsset choiceFont;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TMP_Text[] optionLabels;
    [SerializeField] private TMP_Text panelTitle;
    private RectTransform panelBackground;
    private readonly Queue<int> pendingLevelUps = new Queue<int>();
    private bool hasChosenWeapon;
    private bool hasChosenAuxiliaryWeapons;
    private bool isChoosing;
    private bool auxiliaryUnlockPending;
    private readonly HashSet<int> chosenAuxiliaryIndices = new HashSet<int>();

    private void Awake()
    {
        if (playerExperience == null)
            playerExperience = FindObjectOfType<PlayerExperience>();
        if (weaponController == null)
            weaponController = FindObjectOfType<PlayerWeaponController>();
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
        if (auxiliaryWeaponController == null)
            auxiliaryWeaponController = FindObjectOfType<PlayerAuxiliaryWeaponController>();
        if (auxiliaryWeaponController == null && playerController != null)
            auxiliaryWeaponController = playerController.gameObject.AddComponent<PlayerAuxiliaryWeaponController>();
        if (progressionManager == null)
            progressionManager = FindObjectOfType<GameProgressionManager>();

        bool hasValidSelectionUi = optionButtons != null && optionLabels != null &&
            optionButtons.Length == optionLabels.Length && optionButtons.Length >= 4;
        if (!hasValidSelectionUi)
            Debug.LogError("선택 UI 카드 4개와 각 TMP 라벨을 씬에서 연결해야 합니다.", this);
        else
            SetupPanelLayout();

        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        if (weaponController != null)
            weaponController.SetGameplayActive(false);
        if (auxiliaryWeaponController != null)
            auxiliaryWeaponController.SetGameplayActive(false);
        if (playerController != null)
            playerController.enabled = false;

        Time.timeScale = 0f;
    }

    private void OnEnable()
    {
        if (playerExperience != null)
            playerExperience.OnLevelUp += HandleLevelUp;
        if (progressionManager != null)
            progressionManager.AuxiliaryWeaponsUnlocked += HandleAuxiliaryWeaponsUnlocked;
    }

    private void OnDisable()
    {
        if (playerExperience != null)
            playerExperience.OnLevelUp -= HandleLevelUp;
        if (progressionManager != null)
            progressionManager.AuxiliaryWeaponsUnlocked -= HandleAuxiliaryWeaponsUnlocked;
    }

    private void Start()
    {
        ShowStartingWeaponChoices();
    }

    private void HandleAuxiliaryWeaponsUnlocked()
    {
        if (!hasChosenWeapon || hasChosenAuxiliaryWeapons || isChoosing)
        {
            auxiliaryUnlockPending = true;
            return;
        }

        ShowAuxiliaryWeaponChoices();
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
        Time.timeScale = 0f;
        if (weaponController != null)
            weaponController.SetGameplayActive(false);
        if (auxiliaryWeaponController != null)
            auxiliaryWeaponController.SetGameplayActive(false);
        if (playerController != null)
            playerController.enabled = false;
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
        weaponController.SetGameplayActive(false);
        ContinueAfterChoice();
    }

    private void ShowAuxiliaryWeaponChoices()
    {
        if (auxiliaryWeaponController == null || optionButtons == null)
        {
            Debug.LogError("보조 무기 선택 UI 또는 PlayerAuxiliaryWeaponController가 연결되지 않았습니다.", this);
            ContinueAfterChoice();
            return;
        }

        AuxiliaryWeapon[] weapons = auxiliaryWeaponController.AvailableWeapons;
        if (weapons == null || weapons.Length < PlayerAuxiliaryWeaponController.InitialChoiceCount ||
            optionButtons.Length < weapons.Length)
        {
            Debug.LogError("보조 무기 4종 또는 선택 버튼이 모두 준비되지 않았습니다.", this);
            ContinueAfterChoice();
            return;
        }

        isChoosing = true;
        Time.timeScale = 0f;
        weaponController.SetGameplayActive(false);
        auxiliaryWeaponController.SetGameplayActive(false);
        if (playerController != null)
            playerController.enabled = false;
        levelUpPanel.SetActive(true);

        chosenAuxiliaryIndices.Clear();
        SetPanelTitle("보조 무기 해금! 1개를 선택하세요 (0/1)");
        LayoutOptionCards(weapons.Length);
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i >= weapons.Length || weapons[i] == null)
            {
                optionButtons[i].gameObject.SetActive(false);
                continue;
            }

            int weaponIndex = i;
            string label = $"{weapons[i].DisplayName}\n{weapons[i].Description}";
            BindOption(i, label, () => SelectAuxiliaryWeapon(weaponIndex));
        }
    }

    private void SelectAuxiliaryWeapon(int weaponIndex)
    {
        if (auxiliaryWeaponController == null || chosenAuxiliaryIndices.Contains(weaponIndex) ||
            !auxiliaryWeaponController.TryEquip(weaponIndex))
            return;

        chosenAuxiliaryIndices.Add(weaponIndex);
        AuxiliaryWeapon selectedWeapon = auxiliaryWeaponController.AvailableWeapons[weaponIndex];
        optionButtons[weaponIndex].interactable = false;
        if (optionLabels[weaponIndex] != null)
            optionLabels[weaponIndex].text = $"{selectedWeapon.DisplayName}\n선택됨 ✓";

        if (chosenAuxiliaryIndices.Count >= PlayerAuxiliaryWeaponController.InitialChoiceCount)
        {
            hasChosenAuxiliaryWeapons = true;
            isChoosing = false;
            if (pendingLevelUps.Count > 0)
                ShowNextUpgradeChoice();
            else
                ContinueAfterChoice();
            return;
        }

        SetPanelTitle("보조 무기 1개를 선택하세요 (1/1)");
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
        while (pendingLevelUps.Count > 0 && weapon == null)
            pendingLevelUps.Dequeue();

        if (pendingLevelUps.Count == 0)
        {
            ContinueAfterChoice();
            return;
        }

        List<UpgradeChoice> upgrades = new List<UpgradeChoice>();
        if (weapon.Level < BaseWeapon.MaxLevel)
        {
            foreach (WeaponUpgradeType upgrade in weapon.GetAvailableUpgrades())
                upgrades.Add(UpgradeChoice.Primary(weapon, upgrade));
        }

        if (auxiliaryWeaponController != null)
        {
            foreach (AuxiliaryWeapon auxiliary in auxiliaryWeaponController.EquippedWeapons)
            {
                if (auxiliary.Level >= auxiliary.MaxLevel)
                    continue;

                foreach (AuxiliaryUpgradeType upgrade in auxiliary.GetAvailableUpgrades())
                    upgrades.Add(UpgradeChoice.Auxiliary(auxiliary, upgrade));
            }

            if (hasChosenAuxiliaryWeapons)
            {
                AuxiliaryWeapon[] availableAuxiliary = auxiliaryWeaponController.AvailableWeapons;
                for (int i = 0; i < availableAuxiliary.Length; i++)
                {
                    AuxiliaryWeapon candidate = availableAuxiliary[i];
                    if (candidate == null || auxiliaryWeaponController.IsEquipped(candidate))
                        continue;
                    upgrades.Add(UpgradeChoice.NewAuxiliary(candidate, i));
                }
            }
        }

        if (upgrades.Count == 0)
        {
            pendingLevelUps.Dequeue();
            ShowNextUpgradeChoice();
            return;
        }

        isChoosing = true;
        Time.timeScale = 0f;
        weaponController.SetGameplayActive(false);
        if (auxiliaryWeaponController != null)
            auxiliaryWeaponController.SetGameplayActive(false);
        if (playerController != null)
            playerController.enabled = false;
        levelUpPanel.SetActive(true);

        Shuffle(upgrades);
        int optionCount = Mathf.Min(upgrades.Count, 3);
        SetPanelTitle($"무기 강화 {weapon.Level + 1}단계 강화 선택");
        LayoutOptionCards(optionCount);
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i >= optionCount)
            {
                optionButtons[i].gameObject.SetActive(false);
                continue;
            }

            UpgradeChoice choice = upgrades[i];
            if (choice.newAuxiliaryWeapon != null)
            {
                string label = $"새 보조 무기\n{choice.newAuxiliaryWeapon.DisplayName}\n{choice.newAuxiliaryWeapon.Description}\n선택 시 Lv.1부터 시작";
                BindOption(i, label, () => ShowAuxiliaryAcquisitionChoices(choice));
            }
            else if (choice.primaryWeapon != null)
            {
                int nextLevel = choice.primaryWeapon.Level + 1;
                string label = $"{BaseWeapon.GetUpgradeLabel(choice.primaryUpgrade)}\n{choice.primaryWeapon.WeaponName} Lv.{nextLevel}";
                BindOption(i, label, () => SelectUpgrade(choice));
            }
            else
            {
                int nextLevel = choice.auxiliaryWeapon.Level + 1;
                string label = $"{choice.auxiliaryWeapon.DisplayName}\n{choice.auxiliaryWeapon.GetUpgradeLabel(choice.auxiliaryUpgrade)}\nLv.{nextLevel}";
                BindOption(i, label, () => SelectUpgrade(choice));
            }
        }
    }

    private void SelectUpgrade(UpgradeChoice choice)
    {
        bool applied = choice.primaryWeapon != null
            ? choice.primaryWeapon.ApplyUpgrade(choice.primaryUpgrade)
            : choice.auxiliaryWeapon != null && choice.auxiliaryWeapon.ApplyUpgrade(choice.auxiliaryUpgrade);
        if (applied && pendingLevelUps.Count > 0)
            pendingLevelUps.Dequeue();

        isChoosing = false;
        ShowNextUpgradeChoice();
    }

    private void ShowAuxiliaryAcquisitionChoices(UpgradeChoice choice)
    {
        int equippedCount = auxiliaryWeaponController != null ? auxiliaryWeaponController.EquippedCount : 0;
        SetPanelTitle($"새 보조 무기 · {choice.newAuxiliaryWeapon.DisplayName}");
        int cursor = 0;
        if (equippedCount < PlayerAuxiliaryWeaponController.MaxEquippedWeapons)
            BindOption(cursor++, "빈 슬롯에 추가\n현재 보조 무기는 유지", () => CompleteAuxiliaryChange(-1, choice.newAuxiliaryIndex));

        for (int slot = 0; slot < equippedCount; slot++)
        {
            int slotIndex = slot;
            AuxiliaryWeapon current = auxiliaryWeaponController.GetEquippedAt(slot);
            BindOption(cursor++, $"슬롯 {slot + 1}과 교체\n{current.DisplayName} Lv.{current.Level}",
                () => CompleteAuxiliaryChange(slotIndex, choice.newAuxiliaryIndex));
        }

        BindOption(cursor++, "돌아가기\n강화 선택으로 이동", ReturnToUpgradeChoices);
        for (int i = cursor; i < optionButtons.Length; i++)
            optionButtons[i].gameObject.SetActive(false);
        LayoutOptionCards(cursor);
    }

    private void CompleteAuxiliaryChange(int slotIndex, int weaponIndex)
    {
        bool changed = slotIndex < 0
            ? auxiliaryWeaponController.TryEquip(weaponIndex)
            : auxiliaryWeaponController.ReplaceEquipped(slotIndex, weaponIndex);
        if (!changed)
            return;

        hasChosenAuxiliaryWeapons = true;
        if (pendingLevelUps.Count > 0)
            pendingLevelUps.Dequeue();
        isChoosing = false;
        ShowNextUpgradeChoice();
    }

    private void ReturnToUpgradeChoices()
    {
        isChoosing = false;
        ShowNextUpgradeChoice();
    }

    private void BindOption(int index, string label, UnityEngine.Events.UnityAction callback)
    {
        Button button = optionButtons[index];
        button.gameObject.SetActive(true);
        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callback);

        if (optionLabels[index] != null)
        {
            RectTransform labelRect = optionLabels[index].rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = new Vector2(-64f, -64f);

            optionLabels[index].text = label;
            optionLabels[index].alignment = TextAlignmentOptions.Center;
            optionLabels[index].fontSize = 42;
            optionLabels[index].enableAutoSizing = true;
            optionLabels[index].fontSizeMin = 28;
            optionLabels[index].fontSizeMax = 46;
            optionLabels[index].textWrappingMode = TextWrappingModes.Normal;
            optionLabels[index].overflowMode = TextOverflowModes.Truncate;
            optionLabels[index].font = choiceFont;
            optionLabels[index].color = Color.white;
            optionLabels[index].raycastTarget = false;
        }
    }

    private void SetupPanelLayout()
    {
        Transform panelContent = levelUpPanel.transform.childCount > 0
            ? levelUpPanel.transform.GetChild(0)
            : levelUpPanel.transform;
        panelBackground = panelContent.GetComponent<RectTransform>();
        if (panelBackground != null)
        {
            panelBackground.anchorMin = new Vector2(0.5f, 0.5f);
            panelBackground.anchorMax = new Vector2(0.5f, 0.5f);
            panelBackground.pivot = new Vector2(0.5f, 0.5f);
            panelBackground.anchoredPosition = Vector2.zero;
            panelBackground.sizeDelta = new Vector2(1500f, 640f);
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

        if (panelTitle != null)
        {
            panelTitle.raycastTarget = false;
            panelTitle.alignment = TextAlignmentOptions.Center;
            RectTransform titleRect = panelTitle.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, 45f);
            titleRect.sizeDelta = new Vector2(1100f, 90f);
            panelTitle.fontSize = 50;
            panelTitle.font = choiceFont;
            panelTitle.color = Color.white;
            panelTitle.enableAutoSizing = false;
        }
    }

    private void LayoutOptionCards(int count)
    {
        if (count <= 0)
            return;

        float cardWidth = count == 4 ? 420f : 500f;
        float cardHeight = count == 4 ? 470f : 550f;
        float spacing = count == 4 ? 465f : 540f;
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

    private void ContinueAfterChoice()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        isChoosing = false;
        if (auxiliaryUnlockPending && hasChosenWeapon && !hasChosenAuxiliaryWeapons)
        {
            auxiliaryUnlockPending = false;
            ShowAuxiliaryWeaponChoices();
            return;
        }

        if (hasChosenWeapon)
        {
            if (playerController != null)
                playerController.enabled = true;
            if (weaponController != null)
                weaponController.SetGameplayActive(true);
            if (auxiliaryWeaponController != null && hasChosenAuxiliaryWeapons)
                auxiliaryWeaponController.SetGameplayActive(true);
            Time.timeScale = 1f;
        }
    }

    private static void Shuffle<T>(List<T> values)
    {
        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            T temp = values[i];
            values[i] = values[j];
            values[j] = temp;
        }
    }
}
