using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffChainSkill", menuName = "Data/Unit/Skill/BuffChain")]
public class BuffChainSkill : UnitSkill
{
    private const string BUFF_CHAIN_ATTACKER = "BuffChainAttacker";

    [SerializeField] BuffEffectData _buffEffectData;
    [SerializeField] int _count;
    [SerializeField] float _interval;

    public override void Active(IAttacker attacker)
    {
        Transform target = Priority == Priority.Target ? attacker.GetTarget() : GetTargetSingle(attacker)?.transform;

        if (target == null)
            return;

        BuffChainAttacker buffChainAttacker = Manager.Resources.Instantiate<GameObject>(BUFF_CHAIN_ATTACKER, attacker.GetCenter(), true)
            .GetComponent<BuffChainAttacker>();

        buffChainAttacker.Setup(
            _buffEffectData, attacker, Manager.Resources.Load<GameObject>(EffectAddress), 
            target, _count, _interval, attacker.TargetLayer, AbilityPower
            );
    }

    protected override GameObject GetTargetSingle(IAttacker attacker)
    {
        return base.GetTargetSingle(attacker);
    }

    public override string GetCalculateValueString(UnitStatus status)
    {
        UnitStats stat = status.GetCurrentStat();
        float value = stat.MagicDamage * (AbilityPower / 100);

        if (_buffEffectData.StatType == StatType.CurHp || _buffEffectData.StatType == StatType.Shield)
            return Mathf.RoundToInt(value).ToString();
        else
            return value.ToString("F1");
    }
}
