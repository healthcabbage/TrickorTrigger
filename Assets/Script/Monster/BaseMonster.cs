using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public enum MonsterState
{
    Idle,
    Move,
    Attack,
    Die
}

public class BaseMonster : MonoBehaviour
{
    [Header("monsterdata")]
    [SerializeField] protected MonsterStats stats;
    [SerializeField] protected MonsterReward reward;

    [Header("target")]
    //호박 공격해야해서 타겟을 정해야함
    [SerializeField] protected Pumpkin targetpumpkin;

    [Header("CurrentState")]
    public MonsterState currentstate { get; private set;}

    public float currenthealth { get; private set;}

    public bool isDead { get; private set;}

    //다음공격타이밍
    private float nextattacktime;

    // 초기화 여부
    private bool isInitialized;

    protected virtual void Awake()
    {
        InitializeMonster();
    }

    protected virtual void Update()
    {
        
    }

    protected virtual void InitializeMonster()
    {
        if (stats == null)
        {
                        Debug.LogError(
                "MonsterStats가 연결되지 않았습니다.",
                this
            );

            return;
        }

        currenthealth = stats.maxHealth;

        currentstate = MonsterState.Idle;

        isDead = false;

        isInitialized = true;
    }
}
