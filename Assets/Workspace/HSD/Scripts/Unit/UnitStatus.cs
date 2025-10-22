using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitStatus
{
    public UnitData Data;
    public string Address => $"{Data.Name}_{Level}";
    public int Level;

    public int CombatPower
    {
        get
        {
            if (Data == null)
                return 0;

            return CalculateCombatPower();
        }
    }

    private int CalculateCombatPower()
    {
        UnitStats stat = Data.UnitStats[0];

        float totalPower = (stat.PhysicalDamage + stat.MagicDamage) * (1 + stat.CritChance / 100 * 1.5f) *
            stat.AttackSpeed * (stat.ManaGain / stat.MaxMana) + (stat.PhysicalDefense + stat.MagicDefense) + stat.MaxHealth * 0.15f;

        totalPower *= GetGradePowerWeight();
        totalPower *= GetRolePowerWeight();

        return Mathf.RoundToInt(totalPower);
    }

    private float GetGradePowerWeight()
    {
        return (Data.Grade) switch
        {
            Grade.NORMAL => 1,
            Grade.RARE => 1.1f,
            Grade.UNIQUE => 1.2f,
            Grade.LEGEND => 1.3f,
            _ => 1
        };
    }

    private float GetRolePowerWeight()
    {
        return (Data.ClassSynergy) switch
        {
            ClassType.TANK => 2.2f,
            ClassType.MELEE => 1.3f,
            ClassType.RANGED => 1f,
            ClassType.SUPPORT => 1.2f,
            _ => 1
        };
    }

    public UnitStats GetCurrentStat()
    {
        return Data.GetUnitStat(Level);
    }

    public UnitStatus(UnitData data, int level = 0)
    {
        Data = data;
        Level = level;
    }
}
