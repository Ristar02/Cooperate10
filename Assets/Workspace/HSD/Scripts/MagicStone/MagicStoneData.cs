using UnityEngine;

public abstract class MagicStoneData : ScriptableObject
{
    public Sprite Icon;
    public string Name;
    public SubGrade Grade;
    [TextArea]
    public string Description;
    public GameObject Prefab => Manager.Resources.Load<GameObject>(Address);
    public string Address; // 어드레서블 주소 값

    [SerializeField] private float _effectDuration = 2;
    public float EffectDuration { get => _effectDuration; }

    [SerializeField] private float _value;
    public float Value { get => _value; }

    [SerializeField] private TargetType _targetType;
    public TargetType TargetType { get => _targetType;}

    public abstract void UseMagicStone(Vector2 pos);

    /// <summary>
    /// 적이 1명일때 사용 (랜덤, 보스)
    /// </summary>   
    protected UnitBase GetSingleTarget(Vector2 pos)
    {
        return (TargetType) switch
        { 
            TargetType.RandomEnemy => GetRandomEnemy(pos),
            TargetType.Boss => GetBoss(pos),
            _ => null,
        };
    }

    /// <summary>
    /// 적이 여러명일때 사용 (전체, 아군 전체)
    /// </summary>    
    protected GameObject[] GetMultipleTargets(Vector2 pos)
    {
        return (TargetType) switch
        {
            TargetType.Enemy => GetEnemies(pos),
            TargetType.Ally => GetAllys(pos),
            _ => null,
        };
    }

#region GetTarget
    protected GameObject[] GetEnemies(Vector2 pos)
    {
        return GetTargets(pos, LayerMask.GetMask("Enemy"));
    }

    protected GameObject[] GetAllys(Vector2 pos)
    {
        return GetTargets(pos, LayerMask.GetMask("Player"));
    }

    protected UnitBase GetRandomEnemy(Vector2 pos)
    {
        GameObject[] objs = GetTargets(pos, LayerMask.GetMask("Enemy"));
        if (objs.Length <= 0)
            return null;

        int randIndex = Random.Range(0, objs.Length);

        return ComponentProvider.Get<UnitBase>(objs[randIndex]);
    }    

    protected UnitBase GetBoss(Vector2 pos)
    {
        GameObject[] objs = GetTargets(pos, LayerMask.GetMask("Enemy"));

        if (objs.Length <= 0)
            return null;

        foreach (var obj in objs)
        {
            UnitBase unit = ComponentProvider.Get<UnitBase>(obj);

            if (unit.Status.Level == 2)
            {
                return unit;
            }
        }

        return null;
    }

    protected GameObject[] GetTargets(Vector2 pos, LayerMask layerMask)
    {
        return Utils.GetTargetsNonAlloc(pos, SearchType.Circle, 100, Vector2.zero, layerMask);
    }
#endregion

#region Apply
    protected void ApplyEffectSingle(UnitBase unit, BuffEffectData buffEffectData)
    {
        unit.StatusController.ApplyEffect(buffEffectData, Value, name);
    }
    protected void ApplyEffectMultiple(GameObject[] objs, BuffEffectData buffEffectData)
    {
        foreach (var obj in objs)
        {
            UnitBase unit = ComponentProvider.Get<UnitBase>(obj);
            if (unit != null)
            {
                ApplyEffectSingle(unit, buffEffectData);
            }
        }
    }

    protected void ApplyTickDamageSingle(UnitBase unit, TickDamageData tickDamageData)
    {
        unit.StatusController.TakeTickDamage((int)Value, tickDamageData.TickCount, tickDamageData.TickInterval);
    }
    protected void ApplyTickDamageMultiple(GameObject[] objs, TickDamageData tickDamageData)
    {
        if (objs == null) return;

        foreach (var obj in objs)
        {
            UnitBase unit = ComponentProvider.Get<UnitBase>(obj);
            if (unit != null)
            {
                ApplyTickDamageSingle(unit, tickDamageData);
            }
        }
    }

    protected void ApplyStatSingle(UnitBase unit, StatType statType)
    {
        unit.StatusController.AddStat(statType, Value, name);
    }

    protected void ApplyStatMultiple(GameObject[] objs, StatType statType)
    {
        if (objs == null) return;

        foreach (var obj in objs)
        {
            UnitBase unit = ComponentProvider.Get<UnitBase>(obj);
            if (unit != null)
            {
                ApplyStatSingle(unit, statType);
            }
        }
    }

    protected void ApplyTakeDamageSingle(UnitBase unit)
    {
        unit.StatusController.TakeDamage((int)Value);
    }

    protected void ApplyTakeDamageMultiple(GameObject[] objs)
    {
        if (objs == null) return;

        foreach (var obj in objs)
        {
            UnitBase unit = ComponentProvider.Get<UnitBase>(obj);
            if (unit != null)
            {
                ApplyTakeDamageSingle(unit);
            }
        }
    }
    #endregion

    #region SpawnEffect
    protected virtual void SpawnEffect(Vector2 pos)
    {
        if (string.IsNullOrEmpty(Address))
            return;

        GameObject effect = Manager.Resources.Instantiate<GameObject>(Address, pos, true);

        if (effect == null)
            return;

        Manager.Resources.Destroy(effect, EffectDuration);
    }
#endregion
}
