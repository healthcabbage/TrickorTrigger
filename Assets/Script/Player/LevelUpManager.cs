
using UnityEngine;

public class LevelUpManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerExperience playerExperience;
    [SerializeField] private GameObject levelUpPanel;

    private void Awake()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
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

    private void HandleLevelUp(int level)
    {
        // 게임 일시정지
        Time.timeScale = 0f;

        // 레벨업 UI 표시
        levelUpPanel.SetActive(true);

        Debug.Log($"레벨 {level} 달성!");
    }

    // 보상 선택 완료 시 호출
    public void CompleteRewardSelection()
    {
        levelUpPanel.SetActive(false);

        // 게임 재개
        Time.timeScale = 1f;
    }
}

