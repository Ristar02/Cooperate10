using UnityEngine;


[CreateAssetMenu(fileName = "BuffSkill", menuName = "Data/Unit/Skill/BuffSkill")]
public class BuffSkill : UnitSkill
{
    [Header("Range Setting")]
    [SerializeField] bool _isRange;
    [SerializeField] float _range;

    [Header("Buff")]
    [SerializeField] TargetType TargetType;
    public BuffEffectData BuffEffectData;

    public override void Active(IAttacker attacker)
    {
        base.Active(attacker);

        SpawnEffect(attacker);

        if (TargetType == TargetType.Self)
        {
            attacker.GetStatusController().ApplyEffect(BuffEffectData, AbilityPower, name);
            return;
        }
        
        if (Priority == Priority.None)
        {
            foreach (GameObject target in GetTargetFromTargetType(attacker))
            {
                attacker.GetStatusController().ProvideEffect(BuffEffectData, AbilityPower, 
                    name, ComponentProvider.Get<UnitBase>(target).StatusController);                
            }
        }
        else
        {
            attacker.GetStatusController().ProvideEffect(BuffEffectData, AbilityPower, 
                name, ComponentProvider.Get<UnitBase>(GetTargetPrioty(attacker)).StatusController);            
        }
    }

    protected GameObject[] GetTargetFromTargetType(IAttacker attacker)
    {
        float range = _isRange ? _range : 100;
        
        switch (TargetType)
        {            
            case TargetType.Ally:
                return Utils.GetTargetsNonAlloc(
                    attacker, attacker.GetCenter(), SearchType.Circle, range,
                    Vector2.zero, 0, MaxCount, attacker.GetAllyLayerMask());
            case TargetType.Enemy:
                return Utils.GetTargetsNonAlloc(
                    attacker, attacker.GetCenter(), SearchType.Circle, range,
                    Vector2.zero, 0, MaxCount, attacker.TargetLayer);
            default:
                return null;
        }
    }

    public override string GetCalculateValueString(UnitStatus status)
    {
        UnitStats stat = status.GetCurrentStat();
        float value = stat.MagicDamage * (AbilityPower / 100);

        if (BuffEffectData.StatType == StatType.CurHp || BuffEffectData.StatType == StatType.Shield)
            return Mathf.RoundToInt(value).ToString();
        else
            return value.ToString("F1");
    }

    private GameObject GetTargetPrioty(IAttacker attacker)
    {
        LayerMask targetLayer = TargetType == TargetType.Ally ? attacker.GetAllyLayerMask() : attacker.TargetLayer;        
            
        return Utils.GetTargetsNonAllocSingle(attacker, SearchType.Circle, 100, Vector2.zero, 1, targetLayer, GetPriorityFilter());        
    }

#if UNITY_EDITOR
    public override void DrawGizmos(IAttacker attacker)
    {
        base.DrawGizmos(attacker);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attacker.GetCenter(), _range);
    }
#endif
}