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
        switch (currentstate)
        {
            case MonsterState.Idle:
                UpdateIdle();
                break;
            case MonsterState.Move:
                UpdateMove();
                break;
            case MonsterState.Attack:
                UpdateAttack();
                break;
            case MonsterState.Die:
                UpdateDie();
                break; 
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Die();
        }
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

    protected virtual void ChangeState(MonsterState newState)
    {
        if (currentstate == newState)
            return;

        currentstate = newState;
    }

    protected virtual void UpdateIdle()
    {
        ChangeState(MonsterState.Move);
    }

    protected virtual void UpdateMove()
    {
        if (IsTargetInRange())
        {
            ChangeState(MonsterState.Attack);
            return;
        }

        MoveToPumpkin();
    }

    protected virtual bool IsTargetInRange()
    {
        if (targetpumpkin == null)
            return false;

        Vector3 monsterPosition = transform.position;
        Vector3 pumpkinPosition = targetpumpkin.transform.position;

        monsterPosition.y = 0f;
        pumpkinPosition.y = 0f;

        float distance = Vector3.Distance(monsterPosition, pumpkinPosition);
        return distance <= stats.attackRange;
    }

    protected virtual void MoveToPumpkin()
    {
        if (targetpumpkin == null)
            return;
        
        Vector3 targetPosition = targetpumpkin.transform.position;

        targetPosition.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, stats.movespeed * Time.deltaTime);
    }

    protected virtual void UpdateAttack()
    {
        // 공격 로직 구현
        if (!IsTargetInRange())
        {
            ChangeState(MonsterState.Move);
            return;
        }

        AttackPumpkin();
    }

    protected virtual void AttackPumpkin()
    {
        if (targetpumpkin == null)
            return;

        if (Time.time < nextattacktime)
            return;
        
        targetpumpkin.TakeDamage(stats.attackDamage);

        nextattacktime = Time.time + stats.attackInterval;
    }

    public virtual void TakeDamage(float damage)
    {
        if(isDead)
            return;
        
        currenthealth -= damage;

        if (currenthealth <= 0f)
        {
            currenthealth = 0f;
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;

        ChangeState(MonsterState.Die);

        // 추가적인 사망 처리 로직을 여기에 구현할 수 있습니다.

        Dropreward();

        gameObject.SetActive(false);
    }
    protected virtual void UpdateDie()
    {
    }

    protected virtual void Dropreward()
    {
        if (reward == null)
        {
            Debug.LogWarning(
                "MonsterReward가 연결되지 않았습니다.",
                this
            );

            return;
        }

        // 보상 드롭 로직 구현
        // 예: 플레이어에게 보상을 추가하거나, 아이템을 생성하는 등의 작업 수행
        for (int i = 0; i < reward.coinamountdrop; i++)
        {
            if (reward.coinPrefab == null)
                break;
            
            GameObject coin = Instantiate(reward.coinPrefab, transform.position, Quaternion.identity);

            
        }
    }
}
