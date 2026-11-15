using UnityEngine;

public class MeleeMonster : BaseMonster
{
    protected override void Attack()
    {
        if (targetPumpkin != null)
            targetPumpkin.TakeDamage(_monsterStats.attackDamage);
    }
}
