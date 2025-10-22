using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit_Data", menuName = "Data/Unit/Unit_Data")]
public class UnitData : MetaData
{
    [Header("MetaData")]
    public Grade Grade;
    public string AddressableAddress;
    public GameObject UnitPrefab => Manager.Resources.Load<GameObject>(AddressableAddress);
    public int ID;
    public int PerferredLine;
    public int Cost;

    [Header("AnimationData")]
    public bool isNotChange;
    public AnimatorData AnimatiorData;

    [Header("Attack_Data")]
    public UnitSkill Skill;
    public UnitAttackData AttackData; // Melee, Ranged 등 공격 타입에 따라 다름

    [Header("Unit_Stat")]    
    public UnitStats[] UnitStats; // 3개 1,2,3 성
    public UnitStats[] UpgradeStats; // UnitStats의 레벨 강화 반영

    [Header("Synergy")]
    public ClassType ClassSynergy;
    public Synergy Synergy;

    [Header("Upgrade")]
    public UpgradeUnitData UpgradeData;
    public LevelUpData LevelUpData;
    public UpgradeStatData UpgradeStatData;

    public UnitStats GetUnitStat(int level)
    {
        if (UpgradeStats == null || UpgradeStats.Length < 4)
            return UnitStats[level];

        return UpgradeStats[level];
    }

    public void Init()
    {
        UpgradeData?.Init(Grade, LevelUpData);

        if(UpgradeData != null)
        {
            UpgradeData.OnLevelUp += UpdateUpgradeStats;
            UpdateUpgradeStats();
        }
    }

    private void OnDestroy()
    {
        if(UpgradeData != null)
        {
            UpgradeData.OnLevelUp -= UpdateUpgradeStats;
        }
    }

    public void UpdateUpgradeStats()
    {
        UpgradeStats = new UnitStats[UnitStats.Length];
        for (int i = 0; i < UnitStats.Length; i++)
        {
            UpgradeStats[i] = UnitStats[i].Clone();
        }

        if (UpgradeStatData == null) return;

        var gradeGrowth = UpgradeStatData.StatData.Find(g => g.CharGrade == Grade);
        if (gradeGrowth.Stats == null || gradeGrowth.Stats.Count == 0) return;

        int currentLevel = UpgradeData?.CurrentUpgradeData.UpgradeLevel ?? 0;

        foreach (var levelGrowth in gradeGrowth.Stats)
        {
            if (levelGrowth.Level <= currentLevel)
            {
                foreach (var statGrowth in levelGrowth.Stats)
                {
                    for (int i = 0; i < UpgradeStats.Length; i++)
                    {
                        UpgradeStats[i].AddStat(statGrowth.Type, statGrowth.Value);
                    }
                }
            }
        }
    }

    public UnitDataDTO ToDTO(UnitData data)
    {
        return new UnitDataDTO
        {
            ClassSynergy = (int)data.ClassSynergy,
            Cost = data.Cost,
            Description = data.Description,
            Grade = data.Grade,
            ID = data.ID,
            Name = data.name,
            PrefferedLine = data.PerferredLine,
            Synergy = (int)data.Synergy,
            UnitStats = data.UnitStats
        };
    }
}

[System.Serializable]
public class UnitDataDTO
{
    public int ClassSynergy;
    public int Cost;
    public string Description;
    public Grade Grade;
    public int ID;
    public string Name;
    public int PrefferedLine;
    public int Synergy;
    public UnitStats[] UnitStats;
}