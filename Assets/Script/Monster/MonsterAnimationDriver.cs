using UnityEngine;

/// <summary>
/// Relays common monster events to an Animator. Each monster can use its own
/// Animator Controller or Animator Override Controller with the same parameters.
/// </summary>
public class MonsterAnimationDriver : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Common Animator Parameters")]
    [SerializeField] private string moveSpeedParameter = "MoveSpeed";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string deathTrigger = "Death";

    [Header("Timing")]
    [SerializeField, Min(0f)] private float movementDampTime = 0.1f;
    [SerializeField, Min(0f), Tooltip("사망 애니메이션 후 몬스터 오브젝트를 제거할 때까지의 대기 시간(초)입니다.")]
    private float deathDestroyDelay = 1f;
    [SerializeField, Min(0f)] private float movementThreshold = 0.03f;

    private int moveSpeedHash;
    private int attackHash;
    private int hitHash;
    private int deathHash;
    private bool hasMoveSpeed;
    private bool hasAttackTrigger;
    private bool hasHitTrigger;
    private bool hasDeathTrigger;
    private bool initialized;
    private bool isDead;
    private Vector3 previousPosition;

    public float DeathDestroyDelay => deathDestroyDelay;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (initialized)
            return;

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            return;

        moveSpeedHash = Animator.StringToHash(moveSpeedParameter);
        attackHash = Animator.StringToHash(attackTrigger);
        hitHash = Animator.StringToHash(hitTrigger);
        deathHash = Animator.StringToHash(deathTrigger);

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == moveSpeedHash && parameter.type == AnimatorControllerParameterType.Float)
                hasMoveSpeed = true;
            else if (parameter.nameHash == attackHash && parameter.type == AnimatorControllerParameterType.Trigger)
                hasAttackTrigger = true;
            else if (parameter.nameHash == hitHash && parameter.type == AnimatorControllerParameterType.Trigger)
                hasHitTrigger = true;
            else if (parameter.nameHash == deathHash && parameter.type == AnimatorControllerParameterType.Trigger)
                hasDeathTrigger = true;
        }

        previousPosition = transform.position;
        initialized = true;
    }

    private void LateUpdate()
    {
        if (!initialized || animator == null || !animator.enabled || !hasMoveSpeed || Time.deltaTime <= 0f)
            return;

        Vector3 velocity = (transform.position - previousPosition) / Time.deltaTime;
        previousPosition = transform.position;
        float moveSpeed = !isDead && velocity.magnitude >= movementThreshold ? velocity.magnitude : 0f;
        animator.SetFloat(moveSpeedHash, moveSpeed, movementDampTime, Time.deltaTime);
    }

    public void PlayAttack()
    {
        PlayTrigger(attackHash, hasAttackTrigger);
    }

    public void PlayHit()
    {
        PlayTrigger(hitHash, hasHitTrigger);
    }

    public bool PlayDeath()
    {
        isDead = true;
        if (hasMoveSpeed && animator != null)
            animator.SetFloat(moveSpeedHash, 0f);

        PlayTrigger(deathHash, hasDeathTrigger);
        return hasDeathTrigger;
    }

    public void PlayCustomTrigger(string triggerName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(triggerName))
            return;

        int hash = Animator.StringToHash(triggerName);
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == hash && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(hash);
                animator.SetTrigger(hash);
                return;
            }
        }
    }

    public void ResetForSpawn()
    {
        isDead = false;
        if (animator == null)
            Initialize();
        if (animator == null)
            return;

        if (hasAttackTrigger)
            animator.ResetTrigger(attackHash);
        if (hasHitTrigger)
            animator.ResetTrigger(hitHash);
        if (hasDeathTrigger)
            animator.ResetTrigger(deathHash);
        if (hasMoveSpeed)
            animator.SetFloat(moveSpeedHash, 0f);

        previousPosition = transform.position;
        animator.Rebind();
        animator.Update(0f);
    }

    private void PlayTrigger(int hash, bool hasParameter)
    {
        if (!initialized)
            Initialize();
        if (animator == null || !animator.enabled || !hasParameter)
            return;

        animator.ResetTrigger(hash);
        animator.SetTrigger(hash);
    }
}
