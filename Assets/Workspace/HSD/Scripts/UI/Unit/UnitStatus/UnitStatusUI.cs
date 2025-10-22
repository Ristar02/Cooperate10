using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitStatusUI : MonoBehaviour
{
    [SerializeField] TMP_Text _physicalDamageText;
    [SerializeField] TMP_Text _magicDamageText;
    [SerializeField] TMP_Text _physicalDefenseText;
    [SerializeField] TMP_Text _magicDefenseText;
    [SerializeField] TMP_Text _critChanceText;
    [SerializeField] TMP_Text _cirtDamageText;
    [SerializeField] TMP_Text _attackSpeedText;
    [SerializeField] TMP_Text _attackRangeText;

    public void Setup(UnitStats stat)
    {
        _physicalDamageText.text = stat.PhysicalDamage.ToString();
        _magicDamageText.text = stat.MagicDamage.ToString();
        _physicalDefenseText.text = stat.PhysicalDefense.ToString();
        _magicDefenseText.text = stat.MagicDefense.ToString();
        _critChanceText.text = stat.CritChance.ToString("F1");
        _attackSpeedText.text = stat.AttackSpeed.ToString("F2");
        _attackRangeText.text = stat.AttackRange.ToString();
    }
}
