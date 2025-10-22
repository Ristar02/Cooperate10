using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SynergyDatabase", menuName = "Data/Database/Synergy")]
public class SynergyDatabase : ScriptableObject
{
    public readonly Dictionary<int, SynergyData> _synergyDataDic = new Dictionary<int, SynergyData>(128);

    [SerializeField] UnitSynergyData[] _unitSynergyDatas;
    [SerializeField] ClassSynergyData[] _classSynergyDatas;

    public void Init()
    {        
        if(_unitSynergyDatas.Length == 0 || _classSynergyDatas.Length == 0)
        {
            Debug.Log("시너지 데이터가 설정되지 않았습니다.");
            return;
        }
        foreach (var synergyData in _unitSynergyDatas)
        {
            if (!_synergyDataDic.ContainsKey((int)synergyData.Synergy))
                _synergyDataDic.Add((int)synergyData.Synergy, synergyData);
            else
                Debug.LogWarning($"Duplicate SynergyName: {(int)synergyData.Synergy}");
        }

        foreach (var synergyData in _classSynergyDatas)
        {
            if (!_synergyDataDic.ContainsKey((int)synergyData.Synergy))
                _synergyDataDic.Add((int)synergyData.Synergy, synergyData);
            else
                Debug.LogWarning($"Duplicate SynergyName: {synergyData.Synergy.ToString()}");
        }

        foreach (var synergyData in _synergyDataDic.Values)
        {
            synergyData.Init();
        }
    }

    public SynergyData GetSynergy(int synergyIdx)
    {
        if(!_synergyDataDic.TryGetValue(synergyIdx, out var effect))
        {
            Init();
        }

        return _synergyDataDic[synergyIdx];
    }

    public void ResetSynergys()
    {
        foreach (var synergyData in _synergyDataDic.Values)
        {
            synergyData.Init();
        }
    }
}
