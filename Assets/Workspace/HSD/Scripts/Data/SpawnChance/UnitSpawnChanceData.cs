using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitSpawnChanceData", menuName = "Data/UnitSpawnChance")]
public class UnitSpawnChanceData : ScriptableObject
{
    private Dictionary<UnitData, float> _cachedChances = new Dictionary<UnitData, float>();

    [SerializeField] private Synergy _currentLeaderSynergy;
    public Synergy CurrentLeaderSynergy
    {
        get => _currentLeaderSynergy;
        set => _currentLeaderSynergy = value;
    }

    [SerializeField] private UnitSpawnChance[] UnitSpawnChances;

    [SerializeField] private float leaderWeight = 1.5f; // 고정 규칙

    public void CalculateChances(int currentFloor)
    {
        _cachedChances.Clear();

        UnitData[] allUnits = Manager.Data.PlayerUnitDatas;
        UnitSpawnChance floorChance = UnitSpawnChances[currentFloor];

        foreach (Grade grade in System.Enum.GetValues(typeof(Grade)))
        {
            List<UnitData> candidates = new List<UnitData>();
            foreach (var u in allUnits)
            {
                if (u.Grade == grade)
                    candidates.Add(u);
            }

            if (candidates.Count == 0) continue;

            float rarityChance = GetRarityChance(floorChance, grade);
            int totalCount = candidates.Count;
            float baseChance = rarityChance / totalCount;

            List<UnitData> leaders = new List<UnitData>();
            foreach (var unit in candidates)
            {
                if (unit.Synergy == _currentLeaderSynergy)
                    leaders.Add(unit);
            }

            int leaderCount = leaders.Count;

            if (leaderCount > 0)
            {
                float leaderChance = baseChance * leaderWeight;
                float totalLeaderChance = leaderChance * leaderCount;

                float remainChance = rarityChance - totalLeaderChance;
                float normalChance = remainChance / (totalCount - leaderCount);

                foreach (var unit in candidates)
                {
                    if (leaders.Contains(unit))
                        _cachedChances[unit] = leaderChance;
                    else
                        _cachedChances[unit] = normalChance;
                }
            }
            else
            {
                foreach (var unit in candidates)
                    _cachedChances[unit] = baseChance;
            }
        }
        float total = 0f;
        foreach (var kvp in _cachedChances)
            total += kvp.Value;

        Debug.Log($"Total chance = {total}");
    }


    public UnitData RollUnit()
    {
        float roll = Random.Range(0, 100);
        foreach (var kvp in _cachedChances)
        {
            roll -= kvp.Value;
            if (roll <= 0f)
                return kvp.Key;
        }

        Debug.LogWarning("Roll failed, returning null");
        return null;
    }

    private float GetRarityChance(UnitSpawnChance chance, Grade rarity)
    {
        return rarity switch
        {
            Grade.NORMAL => chance.Normal,
            Grade.RARE => chance.Rare,
            Grade.UNIQUE => chance.Unique,
            Grade.LEGEND => chance.Legendary,
            _ => 0f
        };
    }
}

[System.Serializable]
public struct UnitSpawnChance // 10층까지의 가중치들
{
    public float Normal;
    public float Rare;
    public float Unique;
    public float Legendary;
}
