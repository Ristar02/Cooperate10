using System;
using System.Collections.Generic;
using UnityEngine;

public class SynergyController : MonoBehaviour
{
    private int[] _synergyCounts;
    public event Action<int, int> OnSynergyChanged;

    public void Init()
    {
        _synergyCounts = new int[(int)Synergy.Length];
    }

    public void AddSynergy(Synergy unitSynergy, ClassType classSynergy)
    {
        int unitSynergyIdx = (int)unitSynergy;
        int classSynergyIdx = (int)classSynergy;

        _synergyCounts[unitSynergyIdx]++;
        _synergyCounts[classSynergyIdx]++;        

        OnSynergyChanged?.Invoke(unitSynergyIdx, _synergyCounts[unitSynergyIdx]);
        OnSynergyChanged?.Invoke(classSynergyIdx, _synergyCounts[classSynergyIdx]);
    }

    public void RemoveSynergy(Synergy unitSynergy, ClassType classSynergy)
    {
        int unitSynergyIdx = (int)unitSynergy;
        int classSynergyIdx = (int)classSynergy;

        _synergyCounts[unitSynergyIdx]--;
        _synergyCounts[classSynergyIdx]--;

        OnSynergyChanged?.Invoke(unitSynergyIdx, _synergyCounts[unitSynergyIdx]);
        OnSynergyChanged?.Invoke(classSynergyIdx, _synergyCounts[classSynergyIdx]);
    }

    public int GetSynergyUnitCount(int synergyIdx)
    {
        return _synergyCounts[synergyIdx];
    }
}