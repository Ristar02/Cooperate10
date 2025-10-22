using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeStatData", menuName = "Data/Upgrade/UpgradeStatData")]
public class UpgradeStatData : ScriptableObject
{
    public List<GradeGrowth> StatData;

    public List<StatusGrowth> GetCurrentStatusData(Grade grade, int level)
    {
        GradeGrowth gradeGrowth = StatData.Find(g => g.CharGrade == grade);
        if (gradeGrowth.Equals(default(GradeGrowth)))
        {
            Debug.LogWarning($"[UpgradeStatData] 해당 Grade({grade}) 데이터가 없음");
            return null;
        }

        LevelGrowth levelGrowth = gradeGrowth.Stats.Find(l => l.Level == level);
        if (levelGrowth.Equals(default(LevelGrowth)))
        {
            Debug.LogWarning($"[UpgradeStatData] Grade({grade}) 내 Level({level}) 데이터가 없음");
            return null;
        }

        return levelGrowth.Stats; // 해당 레벨의 StatusGrowth 리스트 반환
    }
}

[Serializable]
public struct GradeGrowth
{
    public Grade CharGrade;
    public List<LevelGrowth> Stats;
}

[Serializable]
public struct LevelGrowth
{
    public int Level;
    public List<StatusGrowth> Stats;
}

[Serializable]
public struct StatusGrowth
{
    public StatType Type;
    public float Value;
}