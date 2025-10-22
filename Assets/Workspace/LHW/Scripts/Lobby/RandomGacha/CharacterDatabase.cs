using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Database", menuName = "Database/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField] private List<UnitData> units = new List<UnitData>();

    private Dictionary<Grade, List<UnitData>> _unitCache;

    private void OnEnable()
    {
        BuildCache();
    }

    /// <summary>
    /// Grade별 캐시 생성
    /// </summary>
    private void BuildCache()
    {
        _unitCache = new Dictionary<Grade, List<UnitData>>();

        foreach (var unit in units)
        {
            if (!_unitCache.ContainsKey(unit.Grade))
            {
                _unitCache[unit.Grade] = new List<UnitData>();
            }
            _unitCache[unit.Grade].Add(unit);
        }
    }

    /// <summary>
    /// 특정 Grade에 해당하는 모든 UnitData 리스트 반환
    /// </summary>
    public List<UnitData> GetUnitsByGrade(Grade grade)
    {
        if (_unitCache == null) BuildCache();
        return _unitCache.ContainsKey(grade) ? _unitCache[grade] : new List<UnitData>();
    }

    /// <summary>
    /// 특정 Grade에 해당하는 UnitData 중 랜덤 1개 반환
    /// </summary>
    public UnitData GetRandomUnitByGrade(Grade grade)
    {
        if (_unitCache == null) BuildCache();

        if (_unitCache.TryGetValue(grade, out var list) && list.Count > 0)
        {
            return list[Random.Range(0, list.Count)];
        }

        return null;
    }
}