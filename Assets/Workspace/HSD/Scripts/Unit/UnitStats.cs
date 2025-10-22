using System;
using UnityEngine;

[System.Serializable]
public class UnitStats
{
    [Header("Status")]
    public int MaxHealth;
    public int MaxMana;
    public int ManaGain;
    public float AttackSpeed;
    public float MoveSpeed;

    [Header("Damage")]
    public int PhysicalDamage;
    public int MagicDamage;

    [Header("CritRate")]
    public int CritChance;

    [Header("Defense")]
    public int PhysicalDefense;
    public int MagicDefense;

    [Header("Range")]
    public float AttackRange;
    public int AttackCount;

    public UnitStats Clone()
    {
        return new UnitStats
        {
            MaxHealth = this.MaxHealth,
            MaxMana = this.MaxMana,
            ManaGain = this.ManaGain,
            AttackSpeed = this.AttackSpeed,
            MoveSpeed = this.MoveSpeed,
            PhysicalDamage = this.PhysicalDamage,
            MagicDamage = this.MagicDamage,
            CritChance = this.CritChance,
            PhysicalDefense = this.PhysicalDefense,
            MagicDefense = this.MagicDefense,
            AttackRange = this.AttackRange,
            AttackCount = this.AttackCount
        };
    }

    public void AddStat(StatType type, float value)
    {
        switch (type)
        {
            case StatType.MaxHealth:
                MaxHealth += (int)value;
                break;
            case StatType.MaxMana:
                MaxMana += (int)value;
                break;
            case StatType.ManaGain:
                ManaGain += (int)value;
                break;
            case StatType.AttackSpeed:
                AttackSpeed += value;
                break;
            case StatType.MoveSpeed:
                MoveSpeed += value;
                break;
            case StatType.PhysicalDamage:
                PhysicalDamage += (int)value;
                break;
            case StatType.MagicDamage:
                MagicDamage += (int)value;
                break;
            case StatType.CritChance:
                CritChance += (int)value;
                break;
            case StatType.PhysicalDefense:
                PhysicalDefense += (int)value;
                break;
            case StatType.MagicDefense:
                MagicDefense += (int)value;
                break;
            case StatType.AttackRange:
                AttackRange += (int)value;
                break;
            case StatType.AttackCount:
                AttackCount += (int)value;
                break;
        }
    }
    public void AddAugments(StatType status, float rate, string name, UnitStatusController controller)
    {
        StatEffectModifier modifier = CalculateAugment(status, rate);
        AugmentManager.Instance.AddAugment(modifier.StatType, modifier.Value, name, controller);
    }

    public StatEffectModifier CalculateAugment(StatType status, float rate)
    {
        StatEffectModifier modifier = new StatEffectModifier();
        modifier.StatType = status;
        switch (modifier.StatType)
        {
            case StatType.MaxHealth: modifier.Value = Mathf.RoundToInt(MaxHealth * (rate / 100)); break;
            case StatType.MaxMana: modifier.Value = Mathf.RoundToInt(MaxMana * (rate / 100)); break;
            case StatType.ManaGain: modifier.Value = Mathf.RoundToInt(ManaGain * (rate / 100)); break;
            case StatType.AttackSpeed: modifier.Value = AttackSpeed * (rate / 100); break;
            case StatType.MoveSpeed: modifier.Value = MoveSpeed * (rate / 100); break;
            case StatType.PhysicalDamage: modifier.Value = Mathf.RoundToInt(PhysicalDamage * (rate / 100)); break;
            case StatType.MagicDamage: modifier.Value = Mathf.RoundToInt(MagicDamage * (rate / 100)); break;
            case StatType.CritChance: modifier.Value = Mathf.RoundToInt(CritChance * (rate / 100)); break;
            case StatType.PhysicalDefense: modifier.Value = Mathf.RoundToInt(PhysicalDefense * (rate / 100)); break;
            case StatType.MagicDefense: modifier.Value = Mathf.RoundToInt(MagicDefense * (rate / 100)); break;
            case StatType.AttackRange: modifier.Value = AttackRange * (rate / 100); break;
            case StatType.AttackCount: modifier.Value = Mathf.RoundToInt(AttackCount * (rate / 100)); break;
            default: modifier.Value = 0; break;
        }

        return modifier;
    }

    public void RemoveAugments(StatType status, string name, UnitStatusController controller)
    {
        AugmentManager.Instance.RemoveAugment(status, name, controller);
    }
}
