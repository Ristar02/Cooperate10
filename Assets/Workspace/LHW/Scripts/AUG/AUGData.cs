using UnityEngine;

[CreateAssetMenu(fileName = "AUGEffect", menuName = "Data/AUG/AUGEffect")]
public class AUGData : MetaData
{
    public string AUGID;
    public SubGrade Grade;

    public EffectTargetType TargetType;
    public ClassType Class;

    public TriggerType Trigger;
    public EffectType EffectType;
    public EffectTime ApplyTime;

    public StatType[] StatTypes;

    public float[] Rate = new float[3];

    public float currentRate;

    /// <summary>
    /// 증강 등급 적용
    /// </summary>
    /// <param name="grade"></param>
    public void ApplyRate(SubGrade grade)
    {
        switch (grade)
        {
            case SubGrade.SILVER: currentRate = Rate[0]; break;
            case SubGrade.GOLD: currentRate = Rate[1]; break;
            case SubGrade.PRISM: currentRate = Rate[2]; break;
            default: currentRate = 0; break;
        }
    }

    #region Status Augment

    /// <summary>
    /// 캐릭터 스테이터스 증강 적용
    /// </summary>
    /// <param name="unit"></param>
    public void ApplyBuffEffect(UnitBase unit)
    {
        ApplyRate(Grade);
        for (int i = 0; i < StatTypes.Length; i++)
        {
            unit.Status.Data.UnitStats[0].AddAugments(StatTypes[i], currentRate, unit.Status.Data.Name, unit.StatusController);
        }
    }

    /// <summary>
    /// 캐릭터 스테이터스 증강 삭제
    /// </summary>
    /// <param name="unit"></param>
    public void RemoveBuffEffect(UnitBase unit)
    {
        for (int i = 0; i < StatTypes.Length; i++)
        {
            unit.Status.Data.UnitStats[0].RemoveAugments(StatTypes[i], unit.Status.Data.Name, unit.StatusController);
        }
    }

    #endregion

    #region Increase Augment

    /// <summary>
    /// 회복 증강의 회복 적용
    /// </summary>
    /// <param name="unit"></param>
    public void ApplyIncreaseEffect(UnitBase unit)
    {
        ApplyRate(Grade);
        if (StatTypes[0] == StatType.MaxHealth)
        {
            int increaseRate = (int)(unit.Status.Data.UnitStats[0].MaxHealth * (1 + currentRate / 100));
            unit.StatusController.IncreaseHealth(increaseRate);
        }
        else if (StatTypes[0] == StatType.MaxMana)
        {
            int increaseRate = (int)(unit.Status.Data.UnitStats[0].MaxMana * (1 + currentRate / 100));
            unit.StatusController.IncreaseMana(increaseRate);
        }
    }

    #endregion
}