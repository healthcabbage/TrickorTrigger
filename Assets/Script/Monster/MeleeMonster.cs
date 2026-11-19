using UnityEngine;

public class MeleeMonster : BaseMonster
{
    protected override void Attack()
    {
        PlayAttackAnimation();
        if (targetPumpkin != null)
            targetPumpkin.TakeDamage(_monsterStats.attackDamage);
    }
}
