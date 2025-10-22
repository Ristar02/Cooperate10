using System.Collections.Generic;
using UnityEngine;

public class AugmentManager : InGameSingleton<AugmentManager>
{
    #region Stat
    [Header("Status")]
    public Stat<int> MaxHealth;
    public Stat<float> AttackSpeed;

    [Header("Damage")]
    public Stat<int> PhysicalDamage;
    public Stat<int> MagicDamage;

    [Header("Defense")]
    public Stat<int> PhysicalDefense;
    public Stat<int> MagicDefense;

    [Header("CritRate")]
    public Stat<int> CritChance;

    [Header("Currency")]
    public Stat<float> GoldBonus;

    [Header("CurrentAugment")]
    public List<AUGData> currentAugment = new();
    // 테스트용
    [SerializeField] private AUGData[] AUGData;

    // 캐릭터 스테이터스 적용
    private Dictionary<UnitBase, HashSet<string>> _appliedStatusAugments = new();

    private Dictionary<AUGData, HashSet<string>> _appliedCurrencyAugments = new();
    #endregion

    private void Start()
    {
        for (int i = 0; i < AUGData.Length; i++)
        {
            SelectAugment(AUGData[i]);
        }
    }

    public void SelectAugment(AUGData data)
    {
        string augment = data.Name;

        for (int i = 0; i < currentAugment.Count; i++)
        {
            if (augment == currentAugment[i].Name && (int)data.Grade <= (int)currentAugment[i].Grade)
            {
                Debug.LogWarning("이미 적용된 증강입니다. 중복 적용이 불가합니다.");
                return;
            }
            else if (augment == currentAugment[i].Name && (int)data.Grade > (int)currentAugment[i].Grade)
            {
                currentAugment.Remove(currentAugment[i]);
                break;
            }
        }

        currentAugment.Add(data);
    }

    public void RemoveAugment(AUGData data)
    {
        currentAugment.Remove(data);
    }

    #region Unit Status Augment

    /// <summary>
    /// 유닛 스테이터스 증강 추가
    /// </summary>
    /// <param name="unit"></param>
    public void ApplyAugment(UnitBase unit)
    {
        for (int i = 0; i < currentAugment.Count; i++)
        {
            if (currentAugment == null || !IsAugmentTarget(unit, i) || currentAugment[i].EffectType != EffectType.Buff_Debuff) continue;

            if (!_appliedStatusAugments.ContainsKey(unit))
                _appliedStatusAugments[unit] = new HashSet<string>();

            if (_appliedStatusAugments[unit].Contains(currentAugment[i].AUGID))
            {
                Debug.LogWarning($"[AugmentManager] {unit.name} 에 {currentAugment[i].AUGID} 는 이미 적용됨.");
                continue;
            }

            _appliedStatusAugments[unit].Add(currentAugment[i].AUGID);
            currentAugment[i].ApplyBuffEffect(unit);
        }
    }

    /// <summary>
    /// 유닛 스테이터스 증강 해제
    /// </summary>
    /// <param name="unit"></param>
    public void ReleaseAugment(UnitBase unit)
    {
        for (int i = 0; i < currentAugment.Count; i++)
        {
            if (currentAugment == null) continue;

            if (_appliedStatusAugments.TryGetValue(unit, out var augments))
            {
                if (augments.Contains(currentAugment[i].AUGID))
                {
                    currentAugment[i].RemoveBuffEffect(unit);
                    augments.Remove(currentAugment[i].AUGID);
                    if (augments.Count == 0)
                        _appliedStatusAugments.Remove(unit);
                }
                else
                {
                    Debug.LogWarning($"[AugmentManager] {unit.name} 에 {currentAugment[i].AUGID} 는 적용되지 않아 해제할 수 없음.");
                }
            }
        }
    }

    /// <summary>
    /// 유닛 스테이터스 증강 적용 여부 판정
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public bool IsAugmentTarget(UnitBase unit, int index)
    {
        if (currentAugment[index] == null) return false;
        if (currentAugment[index].TargetType == EffectTargetType.Ally) return true;
        else if (currentAugment[index].TargetType == EffectTargetType.SameClassType && unit.Status.Data.ClassSynergy == currentAugment[index].Class)
        {
            Debug.Log("클래스가 같음");
            return true;
        }
        // 리더일 경우 추가 필요

        return false;
    }

    #endregion

    #region Unit Increase

    public void ApplyHealAugment(UnitBase unit)
    {
        for (int i = 0; i < currentAugment.Count; i++)
        {
            if (currentAugment == null || !IsAugmentTarget(unit, i) || currentAugment[i].EffectType != EffectType.Increase) continue;

            if (!_appliedStatusAugments.ContainsKey(unit))
                _appliedStatusAugments[unit] = new HashSet<string>();

            if (_appliedStatusAugments[unit].Contains(currentAugment[i].AUGID))
            {
                Debug.LogWarning($"[AugmentManager] {unit.name} 에 {currentAugment[i].AUGID} 는 이미 적용됨.");
                continue;
            }

            _appliedStatusAugments[unit].Add(currentAugment[i].AUGID);
            currentAugment[i].ApplyIncreaseEffect(unit);
        }
    }

    public void ReleaseHealAugment(UnitBase unit)
    {
        for (int i = 0; i < currentAugment.Count; i++)
        {
            if (currentAugment == null) continue;

            if (_appliedStatusAugments.TryGetValue(unit, out var augments))
            {
                if (augments.Contains(currentAugment[i].AUGID))
                {
                    augments.Remove(currentAugment[i].AUGID);

                    if (augments.Count == 0)
                        _appliedStatusAugments.Remove(unit);
                }
                else
                {
                    Debug.LogWarning($"[AugmentManager] {unit.name} 에 {currentAugment[i].AUGID} 는 적용되지 않아 해제할 수 없음.");
                }
            }
        }
    }

    #endregion

    #region Currency Augment

    /// <summary>
    /// 재화 획득 증강 추가
    /// </summary>
    public void ApplyAugment()
    {
        for (int i = 0; i < currentAugment.Count; i++)
        {
            if (currentAugment == null) continue;

            if (!_appliedCurrencyAugments.ContainsKey(currentAugment[i]))
                _appliedCurrencyAugments[currentAugment[i]] = new HashSet<string>();

            if (_appliedCurrencyAugments[currentAugment[i]].Contains(currentAugment[i].AUGID))
            {
                Debug.LogWarning($"[AugmentManager] {currentAugment[i].name} 에 {currentAugment[i].AUGID} 는 이미 적용됨.");
                continue;
            }

            _appliedCurrencyAugments[currentAugment[i]].Add(currentAugment[i].AUGID);
            AddAugment(currentAugment[i].StatTypes[i], currentAugment[i].currentRate, currentAugment[i].Name);
        }
    }

    /// <summary>
    /// 재화 획득 증강 해제
    /// </summary>
    public void ReleaseAugment()
    {
        for (int i = 0; i < currentAugment.Count; i++)
        {
            if (currentAugment == null) return;

            if (_appliedCurrencyAugments.TryGetValue(currentAugment[i], out var augments))
            {
                if (augments.Contains(currentAugment[i].AUGID))
                {
                    RemoveAugment(currentAugment[i].StatTypes[i], currentAugment[i].Name);

                    augments.Remove(currentAugment[i].AUGID);

                    if (augments.Count == 0)
                        _appliedCurrencyAugments.Remove(currentAugment[i]);
                }
                else
                {
                    Debug.LogWarning($"[AugmentManager] {currentAugment[i].name} 에 {currentAugment[i].AUGID} 는 적용되지 않아 해제할 수 없음.");
                }
            }
        }
    }

    #endregion

    #region Augument Management
    public void AddAugment(StatType statType, float value, string source, UnitStatusController controller)
    {
        switch (statType)
        {
            case StatType.MaxHealth:
                MaxHealth.AddModifier((int)value, source);
                controller?.AddStat(statType, (int)value, source);
                break;
            case StatType.PhysicalDamage:
                PhysicalDamage.AddModifier((int)value, source);
                controller?.AddStat(statType, (int)value, source);
                break;
            case StatType.MagicDamage:
                MagicDamage.AddModifier((int)value, source);
                controller?.AddStat(statType, (int)value, source);
                break;
            case StatType.CritChance:
                CritChance.AddModifier((int)value, source);
                controller?.AddStat(statType, (int)value, source);
                break;
            case StatType.PhysicalDefense:
                PhysicalDefense.AddModifier((int)value, source);
                controller?.AddStat(statType, (int)value, source);
                break;
            case StatType.MagicDefense:
                MagicDefense.AddModifier((int)value, source);
                controller?.AddStat(statType, (int)value, source);
                break;
            case StatType.AttackSpeed:
                AttackSpeed.AddModifier(value, source);
                controller?.AddStat(statType, (int)value, source);
                break;
        }
    }

    public void AddAugment(StatType statType, float value, string source)
    {
        switch (statType)
        {
            case StatType.GoldBonus:
                GoldBonus.AddModifier(value, source);
                break;
        }
    }

    public void RemoveAugment(StatType statType, string source, UnitStatusController controller)
    {
        switch (statType)
        {
            case StatType.MaxHealth:
                MaxHealth.RemoveModifier(source);
                controller?.RemoveStat(statType, source);
                break;
            case StatType.PhysicalDamage:
                PhysicalDamage.RemoveModifier(source);
                controller?.RemoveStat(statType, source);
                break;
            case StatType.MagicDamage:
                MagicDamage.RemoveModifier(source);
                controller?.RemoveStat(statType, source);
                break;
            case StatType.CritChance:
                CritChance.RemoveModifier(source);
                controller?.RemoveStat(statType, source);
                break;
            case StatType.PhysicalDefense:
                PhysicalDefense.RemoveModifier(source);
                controller?.RemoveStat(statType, source);
                break;
            case StatType.MagicDefense:
                MagicDefense.RemoveModifier(source);
                controller?.RemoveStat(statType, source);
                break;
            case StatType.AttackSpeed:
                AttackSpeed.RemoveModifier(source);
                controller?.RemoveStat(statType, source);
                break;
        }
    }

    public void RemoveAugment(StatType statType, string source)
    {
        switch (statType)
        {
            case StatType.GoldBonus:
                GoldBonus.RemoveModifier(source);
                break;
        }
    }
    #endregion
}