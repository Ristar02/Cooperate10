using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SynergyEffect", menuName = "Data/Synergy/Effect")]
public class SynergyEffect : ScriptableObject
{
    //[Header("MetaData")]
    private string GuidKey = Guid.NewGuid().ToString();
    public string Key => name;

    [TextArea]
    public string Description;
    public bool IsActivationsClear;
    public bool IsAttack;
    public bool IsSpawn;
    public bool IsBuff;
    public bool IsDelay;
    public bool IsFirstOnly;

    //[Header("Delay")]
    public float DelayTime;

    //[Header("SpawnEffect")]
    public string SynergyEffectAddress;
    public float EffectDuration = 2;

    //[Header("SpawnType (유닛 소환)")]
    public bool IsUnitPosition;         // 소환 위치 정의 (유닛위치 or 전장 중앙)
    public int UnitStatMultiplier { get; private set; }
    public string UnitDataAddress;
    [Range(0, 2)] public int UnitLevel;
    public UnitStats SpawnUnitStats;    // 가중치
    public bool IsMultiplier;           // 시너지 유닛의 Level에 따른 배수 적용 여부

    public SpawnStatType SpawnType;     // 유닛소환 시 스텟 타입 설정
    public Synergy SpawnSynergy;        // 유닛을 소환하는 시너지
    public GameObject SpawnPrefab => Manager.Resources.Load<GameObject>(SpawnAddress);
    public GameObject SpawnEffectPrefab => Manager.Resources.Load<GameObject>(SpawnEffectAddress);
    public string SpawnAddress;
    public string SpawnEffectAddress;

    public EffectApplyType EffectApplyType;

    //[Header("AttackType (공격)")]
    public SpawnPositionType SpawnPositionType;
    public float Power;
    public float AttackDealy = 1;
    public GameObject AttackPrefab => Manager.Resources.Load<GameObject>(AttackAddress);
    public string AttackAddress;

    //[Header("EffectType")]
    public EffectType EffectType;
    public SynergyBuffData[] SynergyBuffDatas;
    public StatEffectModifier[] StatModifiers;

    //[Header("TriggerType")]
    public TriggerType TriggerType;
    public float Interval;
    public uint MaxActivations;

    //[Header("TargetType")]
    public EffectTargetType TargetType;

    //[Header("NextEffect")]
    public SynergyEffect NextEffect;    

    public void ApplyEffect(UnitBase[] units, int synergy)
    {
        if(IsMultiplier)
            UnitStatMultiplier = units.GetSynergyUnitsTotalLevel(SpawnSynergy);

        if (TargetType == EffectTargetType.Cross)
        {
            ActiveCross(units, synergy);
            return;
        }

        if (EffectApplyType == EffectApplyType.Self)
        {
            foreach (var unit in GetTarget(units, synergy))
            {
                unit.StatusController.PassiveController.AddPassiveEffect(this);
            }
        }
        else
        {
            SynergyEffectManager.Instance.GlobalPassiveController.AddPassiveEffect(this, GetTarget(units, synergy), isChange: true, delay: DelayTime);
        }

        if(NextEffect != null && NextEffect.EffectApplyType == EffectApplyType.All)
        {
            SynergyEffectManager.Instance.GlobalPassiveController.AddPassiveEffect(NextEffect, GetTarget(units, synergy), isChange: true, delay: DelayTime);
        }
    }

    public void RemoveEffect(UnitBase[] units, int synergy)
    {
        if (EffectApplyType == EffectApplyType.Self)
        {
            foreach (var unit in GetTarget(units, synergy))
            {
                unit.StatusController.PassiveController.RemovePassiveEffect(this);
            }
        }
        else
        {
            SynergyEffectManager.Instance.GlobalPassiveController.RemovePassiveEffect(this);
        }

        if (NextEffect != null && NextEffect.EffectApplyType == EffectApplyType.All)
        {
            SynergyEffectManager.Instance.GlobalPassiveController.RemovePassiveEffect(NextEffect);
        }
    }

    private void ActiveCross(UnitBase[] units, int synergy)
    {
        foreach (UnitBase unit in units)
        {
            if (unit == null) continue;

            int unitSynergy = (int)unit.Status.Data.Synergy;
            int unitClassSynergy = (int)unit.Status.Data.ClassSynergy;

            if (!(unitSynergy == synergy) && !(unitClassSynergy == synergy))
                continue;

            int synergyCount = 0;
            var currentSlot = unit.CurrentSlot;

            var directions = new Vector2Int[]
            {
                    new Vector2Int(0, 1),
                    new Vector2Int(0, -1),
                    new Vector2Int(-1, 0),
                    new Vector2Int(1, 0)
            };

            foreach (var dir in directions)
            {
                var targetPos = new Vector2Int(currentSlot.x + dir.x, currentSlot.y + dir.y);

                // 해당 위치에 유닛이 있는지 찾기                
                var targetUnit = units.FirstOrDefault(u => u != null &&
                                                           u.CurrentSlot.x == targetPos.x &&
                                                           u.CurrentSlot.y == targetPos.y);

                if (targetUnit != null)
                {
                    unitSynergy = (int)targetUnit.Status.Data.Synergy;
                    unitClassSynergy = (int)targetUnit.Status.Data.ClassSynergy;

                    if (unitSynergy == synergy || unitClassSynergy == synergy)
                    {
                        synergyCount++;
                    }
                }
            }

            if (synergyCount == 0)
                continue;

            unit.StatusController.PassiveController.AddPassiveEffect(this, synergyCount, true);
        }
    }

    protected UnitBase[] GetTarget(UnitBase[] units, int synergy)
    {
        List<UnitBase> unitBases = new List<UnitBase>();

        if (TargetType == EffectTargetType.Ally)
        {
            foreach (var unit in units)
            {
                if(unit != null)
                    unitBases.Add(unit);
            }
            return unitBases.ToArray();
        }            

        switch (TargetType)
        {
            case EffectTargetType.SameSynergy:
                foreach (var unit in units)
                {
                    if (unit == null) continue;

                    if ((int)unit.Status.Data.Synergy == synergy ||
                        (int)unit.Status.Data.ClassSynergy == synergy)
                        unitBases.Add(unit);
                }
                break;
            case EffectTargetType.Column:
                var targetColumns = new HashSet<int>();

                foreach (var unit in units)
                {
                    if (unit == null) continue;

                    if ((int)unit.Status.Data.Synergy == synergy ||
                        (int)unit.Status.Data.ClassSynergy == synergy)
                    {
                        int column = unit.CurrentSlot.x;
                        targetColumns.Add(column);
                    }
                }

                foreach (var unit in units)
                {
                    if (unit == null) continue;

                    int unitColumn = unit.CurrentSlot.x;
                    if (targetColumns.Contains(unitColumn))
                    {
                        unitBases.Add(unit);
                    }
                }
                break;
            case EffectTargetType.Row:
                var targetRows = new HashSet<int>();

                foreach (var unit in units)
                {
                    if (unit == null) continue;

                    if ((int)unit.Status.Data.Synergy == synergy ||
                        (int)unit.Status.Data.ClassSynergy == synergy)
                    {
                        int Row = unit.CurrentSlot.y;
                        targetRows.Add(Row);
                    }
                }

                foreach (var unit in units)
                {
                    if (unit == null) continue;

                    int unitRow = unit.CurrentSlot.y;
                    if (targetRows.Contains(unitRow))
                    {
                        unitBases.Add(unit);
                    }
                }
                break;
            case EffectTargetType.ColumnAndRow:
                var targetCols = new HashSet<int>();
                var targetRowsSet = new HashSet<int>();
                foreach (var unit in units)
                {
                    if (unit == null) continue;

                    if ((int)unit.Status.Data.Synergy == synergy ||
                        (int)unit.Status.Data.ClassSynergy == synergy)
                    {
                        int column = unit.CurrentSlot.x;
                        int row = unit.CurrentSlot.y;
                        targetCols.Add(column);
                        targetRowsSet.Add(row);
                    }
                }
                foreach (var unit in units)
                {
                    if (unit == null) continue;

                    int unitColumn = unit.CurrentSlot.x;
                    int unitRow = unit.CurrentSlot.y;
                    if (targetCols.Contains(unitColumn) || targetRowsSet.Contains(unitRow))
                    {
                        unitBases.Add(unit);
                    }
                }
                break;
        }

        return unitBases.ToArray();
    }
}