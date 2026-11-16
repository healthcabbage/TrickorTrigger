using UnityEngine;

public class MonsterBleedEffect : MonoBehaviour
{
    private BaseMonster monster;
    private GameObject source;
    private float damagePerTick;
    private float tickInterval;
    private float duration;
    private float elapsed;
    private float tickTimer;

    public static void Apply(
        BaseMonster target,
        float damagePerTick,
        float tickInterval,
        float duration,
        GameObject source)
    {
        if (target == null || target.IsDead || damagePerTick <= 0f || duration <= 0f)
            return;

        MonsterBleedEffect effect = target.GetComponent<MonsterBleedEffect>();
        if (effect == null)
            effect = target.gameObject.AddComponent<MonsterBleedEffect>();

        effect.monster = target;
        effect.source = source;
        effect.damagePerTick = damagePerTick;
        effect.tickInterval = Mathf.Max(0.01f, tickInterval);
        effect.duration = duration;
        effect.elapsed = 0f;
        effect.tickTimer = 0f;
    }

    private void Update()
    {
        if (monster == null || monster.IsDead)
        {
            Destroy(this);
            return;
        }

        elapsed += Time.deltaTime;
        tickTimer += Time.deltaTime;
        while (tickTimer >= tickInterval && elapsed <= duration)
        {
            tickTimer -= tickInterval;
            monster.TakeDamage(new DamageInfo
            {
                damage = damagePerTick,
                attacker = source,
                attackSource = source
            });

            if (monster == null || monster.IsDead)
            {
                Destroy(this);
                return;
            }
        }

        if (elapsed >= duration)
            Destroy(this);
    }
}
