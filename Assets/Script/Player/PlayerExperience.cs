
using System;
using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private int currentLevel = 1;

    [Header("Experience")]
    [SerializeField] private int currentExperience = 0;
    [SerializeField] private int baseRequiredExperience = 10;
    [SerializeField] private float experienceGrowthRate = 1.2f;

    public int CurrentLevel => currentLevel;
    public int CurrentExperience => currentExperience;
    public int RequiredExperience { get; private set; }

    // 레벨업 이벤트
    public event Action<int> OnLevelUp;

    private void Awake()
    {
        UpdateRequiredExperience();
    }

    // 경험치 획득
    public void AddExperience(int amount)
    {
        if (amount <= 0)
            return;

        currentExperience += amount;

        Debug.Log($"경험치 획득! 현재 경험치: {currentExperience}/{RequiredExperience}");

        // 레벨업 조건 확인
        while (currentExperience >= RequiredExperience)
        {
            LevelUp();
        }
    }

    // 레벨업
    private void LevelUp()
    {
        currentExperience -= RequiredExperience;
        currentLevel++;

        UpdateRequiredExperience();

        Debug.Log($"레벨 업! 현재 레벨: {currentLevel}");

        OnLevelUp?.Invoke(currentLevel);
    }

    // 다음 레벨에 필요한 경험치 계산
    private void UpdateRequiredExperience()
    {
        RequiredExperience = Mathf.CeilToInt(
            baseRequiredExperience *
            Mathf.Pow(experienceGrowthRate, currentLevel - 1)
        );
    }
}
