using TMPro;
using System.Collections.Generic;
using System.Collections;
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

    [Header("Turning")]
    [SerializeField, Min(0f)] private float turnSpeed = 360f;

    [Header("Drop Settings")]
    [SerializeField] private float dropRadius = 1.5f;
    [SerializeField] private float minDropDistance = 0.5f;
    [SerializeField] private int maxPositionAttempts = 30;

    [Header("Hit Reaction")]
    [SerializeField] private bool ignoreKnockback;

    protected bool isDead;
    protected bool isInitialized;
    private float attackTimer;
    private Animator monsterAnimator;
    private Vector3 deathWorldPosition;
    private bool lockDeathPosition;
    private static readonly int MoveSpeedId = Animator.StringToHash("MoveSpeed");
    private static readonly int AttackId = Animator.StringToHash("Attack");
    private static readonly int DeathId = Animator.StringToHash("Death");
    private static readonly int HitId = Animator.StringToHash("Hit");

    public float CurrentHealth => _currentHealth;
    public bool IsDead => isDead;
    public MonsterState CurrentState => _currentState;
    public event System.Action<float> HealthChanged;

    protected virtual void Awake()
    {
        monsterAnimator = GetComponentInChildren<Animator>();
        if (monsterAnimator != null && monsterAnimator.runtimeAnimatorController == null)
            monsterAnimator = null;

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

        UpdateAnimatorMovement();

        if (Input.GetKeyDown(KeyCode.K))
        {
            Die();
        }
    }

    protected virtual void LateUpdate()
    {
        if (lockDeathPosition)
            transform.position = deathWorldPosition;
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

        FaceTargetPumpkin();

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
        FaceTargetPumpkin();

        if (!IsTargetInAttackRange())
            return;

        attackTimer += Time.deltaTime;
        if (attackTimer < _monsterStats.attackInterval)
            return;

        attackTimer = 0f;
        Attack();
    }

    protected virtual void FaceTargetPumpkin()
    {
        if (targetPumpkin == null)
            return;

        Vector3 direction = targetPumpkin.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime);
    }

    protected bool IsTargetInAttackRange()
    {
        if (targetPumpkin == null)
        {
            ChangeState(MonsterState.Idle);
            return false;
        }

        if (Vector3.Distance(transform.position, targetPumpkin.transform.position) > _monsterStats.attackRange)
        {
            ChangeState(MonsterState.Move);
            return false;
        }

        return true;
    }

    protected virtual void Attack() { }

    protected void PlayAttackAnimation()
    {
        if (monsterAnimator == null)
            return;
        monsterAnimator.SetTrigger(AttackId);
    }

    private void UpdateAnimatorMovement()
    {
        if (monsterAnimator == null || isDead)
            return;

        monsterAnimator.SetFloat(MoveSpeedId, _currentState == MonsterState.Move ? 1f : 0f);
    }

    private float GetDeathAnimationDuration()
    {
        if (monsterAnimator == null || monsterAnimator.runtimeAnimatorController == null)
            return 1f;

        foreach (AnimationClip clip in monsterAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.IndexOf("death", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                clip.name.IndexOf("dead", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return clip.length;
        }

        return 1f;
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
        HealthChanged?.Invoke(_currentHealth);

        if (_currentHealth <= 0f)
        {
            Die();
            return;
        }

        OnHit(damage);
    }

    //몬스터가 공격을 받았을 때 호출되는 로직
    protected virtual void OnHit(DamageInfo damageInfo)
    {
        if (monsterAnimator != null)
            monsterAnimator.SetTrigger(HitId);
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

    public virtual void ApplyKnockback(Vector3 direction, float power)
    {
        if (ignoreKnockback || isDead || power <= 0f || direction.sqrMagnitude <= 0.001f)
            return;

        direction.y = 0f;
        transform.position += direction.normalized * power;
    }

    protected virtual void Die(bool dropReward = true)
    {
        if (isDead)
            return;
        
        isDead = true;
        deathWorldPosition = transform.position;
        lockDeathPosition = monsterAnimator != null;

        ChangeState(MonsterState.Dead);
        StopDeathPhysics();
        IgnorePlayerCollision();

        if (dropReward)
            DropReward();

        if (monsterAnimator != null)
        {
            monsterAnimator.SetBool(DeathId, true);
            int deathStateHash = Animator.StringToHash("Base Layer.Death");
            if (monsterAnimator.HasState(0, deathStateHash))
                monsterAnimator.CrossFadeInFixedTime(deathStateHash, 0.05f, 0, 0f);
            StartCoroutine(DestroyAfterDeathAnimation());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void StopDeathPhysics()
    {
        // Animator 사망 클립이 재생되는 동안 물리 루트가 지면 아래로
        // 떨어지지 않도록 고정합니다. 애니메이터가 없는 몬스터만 물리로 처리합니다.
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody monsterRigidbody in rigidbodies)
        {
            monsterRigidbody.linearVelocity = Vector3.zero;
            monsterRigidbody.angularVelocity = Vector3.zero;
            monsterRigidbody.isKinematic = monsterAnimator != null;
            monsterRigidbody.useGravity = monsterAnimator == null;
            monsterRigidbody.detectCollisions = true;
            monsterRigidbody.WakeUp();
        }
    }

    private void IgnorePlayerCollision()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null)
            return;

        Collider[] monsterColliders = GetComponentsInChildren<Collider>(true);
        Collider[] playerColliders = player.GetComponentsInChildren<Collider>(true);

        // 플레이어와의 충돌만 무시합니다. 지형이나 다른 몬스터와의 충돌은 유지됩니다.
        foreach (Collider monsterCollider in monsterColliders)
        {
            if (monsterCollider == null || !monsterCollider.enabled)
                continue;

            foreach (Collider playerCollider in playerColliders)
            {
                if (playerCollider != null && playerCollider.enabled)
                    Physics.IgnoreCollision(monsterCollider, playerCollider, true);
            }
        }
    }

    private IEnumerator DestroyAfterDeathAnimation()
    {
        if (monsterAnimator != null)
        {
            int deathStateHash = Animator.StringToHash("Base Layer.Death");
            float timeout = Time.time + GetDeathAnimationDuration() + 2f;

            while (Time.time < timeout && monsterAnimator != null)
            {
                AnimatorStateInfo state = monsterAnimator.GetCurrentAnimatorStateInfo(0);
                if (state.fullPathHash == deathStateHash && state.normalizedTime >= 1f)
                    break;

                yield return null;
            }
        }

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
            candidate.y = 0.5f;

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
        Vector3 fallbackPosition = transform.position;
        fallbackPosition.y = 0.5f;
        return fallbackPosition;
    }

    public virtual void OnSpawn()
    {
        targetPumpkin = null;
        attackTimer = 0f;

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
