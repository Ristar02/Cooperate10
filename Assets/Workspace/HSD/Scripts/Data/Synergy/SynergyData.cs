using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SynergyLevelData
{
    public int SynergyNeedCount;
    public SynergyEffect[] Effects;
}

public abstract class SynergyData : ScriptableObject
{
    public Sprite Icon;
    public string SynergyName;
    [TextArea]
    public string Description;
    public SynergyLevelData[] SynergyLevelData;

    private SynergyEffect[] _currentEffects;
    public int CurrentUpgradeIdx;
    protected int _synergy;

    public virtual void Init()
    {
        _currentEffects = new SynergyEffect[SynergyLevelData[0].Effects.Length];
        CurrentUpgradeIdx = -1;
    }

    public void Check(int newCount, UnitBase[] units)
    {
        SynergyEffect[] newEffects = null;

        CurrentUpgradeIdx = -1;

        for (int i = 0; i < SynergyLevelData.Length; i++)
        {
            if (newCount >= SynergyLevelData[i].SynergyNeedCount)
            {
                newEffects = SynergyLevelData[i].Effects;
                CurrentUpgradeIdx = i;
            }
            else
            {
                break;
            }
        }

        if (newEffects == null)
            return;

        foreach (var effect in _currentEffects)
        {
            effect?.RemoveEffect(units, _synergy);
        }

        foreach (var effect in newEffects)
        {
            effect?.ApplyEffect(units, _synergy);
        }

        _currentEffects = newEffects;
    }
}
