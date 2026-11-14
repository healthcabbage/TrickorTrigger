using TMPro;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] protected MonsterStats _monsterStats;
    [SerializeField] protected MonsterReward _monsterReward;

    [Header("Monster State")]
    protected float _currentHealth;
    protected MonsterState _currentState;

    [Header("Target")]
    [SerializeField] protected Pumpkin targetPumpkin;

    [Header("Drop Settings")]
    [SerializeField] private float dropRadius = 1.5f;
    [SerializeField] private float minDropDistance = 0.5f;
    [SerializeField] private int maxPositionAttempts = 30;

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

        if (isDead && newState != MonsterState.Dead)
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

        Vector3 direction =
            targetPumpkin.transform.position - transform.position;

        direction.y = 0f;
        direction.Normalize();

        transform.position += direction
            * _monsterStats.movespeed
            * Time.deltaTime;
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

        // 코인과 사탕이 서로 겹치지 않도록 위치를 공유
        List<Vector3> usedPositions = new List<Vector3>();

        SpawnDrop(
            _monsterReward.coinPrefab,
            _monsterReward.coindropcount,
            usedPositions
        );

        SpawnDrop(
            _monsterReward.candyPrefab,
            _monsterReward.candydropcount,
            usedPositions,
            _monsterReward.candyamountdrop
        );
    }

    protected virtual void SpawnDrop(GameObject prefab, int count, List<Vector3> usedPositions, int experienceAmount = 0)
    {
        if (prefab == null || count <= 0)
                return;

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPosition = GetDropPosition(usedPositions);

                GameObject dropObject = Instantiate(
                    prefab,
                    spawnPosition,
                    Quaternion.identity
                );

                DropItem dropItem = dropObject.GetComponent<DropItem>();

                if (dropItem != null)
                {
                    dropItem.Initialize(experienceAmount);
                }

                usedPositions.Add(spawnPosition);
            }
    }

    private Vector3 GetDropPosition(List<Vector3> usedPositions)
    {
        for (int attempt = 0; attempt < maxPositionAttempts; attempt++)
        {
            // 몬스터 주변의 랜덤한 위치
            Vector2 randomOffset =
                Random.insideUnitCircle * dropRadius;

            Vector3 candidate = transform.position +
                new Vector3(randomOffset.x, 0f, randomOffset.y);

            bool isOverlapping = false;

            foreach (Vector3 usedPosition in usedPositions)
            {
                float distance = Vector3.Distance(
                    candidate,
                    usedPosition
                );

                if (distance < minDropDistance)
                {
                    isOverlapping = true;
                    break;
                }
            }

            if (!isOverlapping)
                return candidate;
        }

        // 적절한 위치를 찾지 못하면 마지막 위치 반환
        return transform.position;
    }

    public virtual void OnSpawn()
    {
        targetPumpkin = null;

        InitializeMonster();
    }

    public virtual void OnDespawn()
    {
        targetPumpkin = null;

        _currentState = MonsterState.Dead;

        isInitialized = false;

        isDead = true;
    }
}
