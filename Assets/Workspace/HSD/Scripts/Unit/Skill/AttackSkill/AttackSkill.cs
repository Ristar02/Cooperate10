using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSkill : UnitSkill
{
    [Header("Type")]
    public DamageType DamageType;

    protected override GameObject GetTargetSingle(IAttacker attacker)
    {
        var target = Utils.GetTargetsNonAllocSingle(attacker, SearchType.Circle, 100, Vector2.zero, 1, attacker.TargetLayer, GetPriorityFilter());
        return target;
    }
}
