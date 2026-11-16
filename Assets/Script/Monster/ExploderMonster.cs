using UnityEngine;

public class ExploderMonster : BaseMonster
{
    [Header("Self Destruct")]
    [SerializeField] private float fuseDuration = 2f;
    [SerializeField] private float explosionRadius = 2f;

    private float fuseTimer;

    protected override void UpdateAttack()
    {
        if (!IsTargetInAttackRange())
        {
            fuseTimer = 0f;
            return;
        }

        fuseTimer += Time.deltaTime;
        if (fuseTimer >= fuseDuration)
            Explode();
    }

    private void Explode()
    {
        if (isDead)
            return;

        if (targetPumpkin != null &&
            Vector3.Distance(transform.position, targetPumpkin.transform.position) <= explosionRadius)
        {
            targetPumpkin.TakeDamage(_monsterStats.attackDamage);
        }

        Die(dropReward: false);
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        fuseTimer = 0f;
    }
}
