using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public enum MonsterState
{
    Idle,
    Move,
    Attack,
    Dead
}

public class BaseMonster : MonoBehaviour
{
    [Header("Monster Data")]
    [SerializeField] private MonsterStats _monsterStats;
    [SerializeField] private MonsterReward _monsterReward;

    [Header("Monster State")]
    protected float _currentHealth;
    protected MonsterState _currentState;

    [Header("Target")]
    [SerializeField] protected Pumpkin targetPumpkin;

    protected bool isDead;
    protected bool isInitialized;

    public float CurrentHealth => _currentHealth;
    public bool IsDead => isDead;
    public MonsterState CurrentState => _currentState;

    protected virtual void Awake()
    {
        InitializeMonster();
    }

    protected virtual void Update()
    {
        if (!isInitialized || isDead)
            return;

        switch (_currentState)
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
            case MonsterState.Dead:
                UpdateDead();
                break;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Die();
        }
    }

    public virtual void InitializeMonster()
    {
        if (_monsterStats == null)
        {
            Debug.LogError("Monster Stats가 연결되지 않았습니다.", this);

            isInitialized = false;
            return;
        }

        _currentHealth = _monsterStats.maxHealth;

        _currentState = MonsterState.Idle;

        isDead = false;

        isInitialized = true;
    }

    protected virtual void ChangeState(MonsterState newState)
    {
        if (_currentState == newState)
            return;

        _currentState = newState;
    }

    //몬스터의 목표 설정
    public virtual void SetTarget(Pumpkin pumpkin)
    {
        if (pumpkin == null)
        {
            Debug.LogError("Target Pumpkin이 연결되지 않았습니다.", this);
            return;
        }

        targetPumpkin = pumpkin;

        ChangeState(MonsterState.Move);
    }

    //호박에게 이동하는 로직
    protected virtual void MoveToTarget()
    {
        if (targetPumpkin == null)
            return;

        //몬스터 이동 로직 구현
        Vector3 targetPosition = targetPumpkin.transform.position - transform.position;

        targetPosition.y = 0f;
        targetPosition.Normalize();

        transform.position += targetPosition * _monsterStats.movespeed * Time.deltaTime;
    }

    protected virtual void UpdateIdle()
    {
        if (targetPumpkin == null)
        {
            ChangeState(MonsterState.Idle);
            return;
        }

        ChangeState(MonsterState.Move);
    }

    protected virtual void UpdateMove()
    {
        if (targetPumpkin == null)
        {
            ChangeState(MonsterState.Idle);
            return;
        }

        float distance = Vector3.Distance(transform.position, targetPumpkin.transform.position);

        if (distance <= _monsterStats.attackRange)
        {
            ChangeState(MonsterState.Attack);
            return;
        }

        MoveToTarget();
    }

    protected virtual void UpdateAttack()
    {
        if (targetPumpkin == null)
        {
            ChangeState(MonsterState.Idle);
            return;
        }

        //공격 로직은 자식에게서
    }

    protected virtual void UpdateDead()
    {
        
    }

    //몬스터가 데미지를 받는 로직
    public virtual void TakeDamage(DamageInfo damage)
    {
        if (!isInitialized || isDead)
            return;
        
        if (damage.damage <= 0f)
            return;
        
        _currentHealth -= damage.damage;

        _currentHealth = Mathf.Max(0f, _currentHealth);

        OnHit(damage);

        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    //몬스터가 공격을 받았을 때 호출되는 로직
    protected virtual void OnHit(DamageInfo damageInfo)
    {
        // 피격 효과
        Debug.Log($"{gameObject.name} 피격!");

        // 넉백 처리
        if (damageInfo.knockbackPower > 0f)
        {
            ApplyKnockback(
                damageInfo.knockbackDirection,
                damageInfo.knockbackPower
            );
        }
    }

    protected virtual void ApplyKnockback(Vector3 direction,float power)
    {
        // 추후 Rigidbody를 이용한 넉백 구현
    }

    protected virtual void Die()
    {
        if (isDead)
            return;
        
        isDead = true;

        ChangeState(MonsterState.Dead);

        //보상 지급
        DropReward();

        Destroy(gameObject);
    }

    protected virtual void DropReward()
    {
        if (_monsterReward == null)
            return;

        // 코인 생성
        SpawnDrop(
            _monsterReward.coinPrefab,
            _monsterReward.coindropcount
        );

        // 사탕 생성
        SpawnDrop(
            _monsterReward.candyPrefab,
            _monsterReward.candydropcount
        );
    }

    protected virtual void SpawnDrop(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0)
            return;

        for (int i = 0; i < count; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                0f,
                Random.Range(-0.5f, 0.5f)
            );

            Vector3 spawnPosition = transform.position + randomOffset;

            Instantiate(prefab, spawnPosition, Quaternion.identity);
        }
    }
}
