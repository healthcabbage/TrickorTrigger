using UnityEngine;

public class MeleeMonster : BaseMonster
{
    [Header("Melee Attack")]
    [SerializeField] private float attackCooldown = 0;

    private float attackTimer;

    protected override void Update()
    {
        base.Update();
    }

    protected override void UpdateAttack()
    {
        base.UpdateAttack();

        float distance = Vector3.Distance(transform.position, targetPumpkin.transform.position);

        if (distance > _monsterStats.attackRange)
        {
            ChangeState(MonsterState.Move);
            return;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0;
            //공격 로직은 자식에게서
            Attack();
        }
    }

    private void Attack()
    {
        if (targetPumpkin == null)
            return;

        targetPumpkin.TakeDamage(_monsterStats.attackDamage);
    }

        public override void OnSpawn()
    {
        base.OnSpawn();
        attackTimer = attackCooldown;
    }
}
