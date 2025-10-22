using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LineSkill", menuName = "Data/Unit/Skill/LineSkill")]
public class PierceLineSkill : AttackSkill
{
    [Header("Line_Skill")]
    private const string CHAIN_LINE_ATTACKER = "ChainLineAttacker";
    [SerializeField] int _count;
    [SerializeField] float _interval;
    [SerializeField] int _ratio = 10;
    [SerializeField] float _attackThickness = 1;

    public override void Active(IAttacker attacker)
    {
        base.Active(attacker);

        Transform target = Priority == Priority.Target ? attacker.GetTarget() : GetTargetSingle(attacker)?.transform;

        if (target == null)
            return;

        GameObject effect = Manager.Resources.Load<GameObject>(EffectAddress);
        ChainLineAttacker chainLineAttacker = Manager.Resources.Instantiate<GameObject>(CHAIN_LINE_ATTACKER, attacker.GetCenter())
            .GetComponent<ChainLineAttacker>();

        chainLineAttacker.Setup(
            attacker, effect, target, _count, _interval, attacker.TargetLayer,
            PhysicalPower, AbilityPower, DamageType, _attackThickness, _ratio
            );
    }

    protected override GameObject GetTargetSingle(IAttacker attacker)
    {
        return base.GetTargetSingle(attacker);
    }

#if UNITY_EDITOR
    public override void DrawGizmos(IAttacker attacker)
    {
        Gizmos.color = Color.magenta;
        Vector2 size = new Vector2(2, _attackThickness);
        Gizmos.DrawWireCube(attacker.GetCenter(), size);        
    }
#endif
}
