using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoUI : MonoBehaviour
{  
    [Header("UI")]
    [SerializeField] Image _unitIcon;
    [SerializeField] TMP_Text _unitNameText;
    [SerializeField] TMP_Text _levelText;
    [SerializeField] TMP_Text _powerText;
    [SerializeField] Image _synergyImage;
    [SerializeField] TMP_Text _synergyNameText;
    [SerializeField] Image _classImage;
    [SerializeField] TMP_Text _classNameText;

    [Header("HP_MP")]
    [SerializeField] Slider _hpSlider;
    [SerializeField] TMP_Text _hpText;
    [SerializeField] Slider _mpSlider;
    [SerializeField] TMP_Text _mpText;

    [Header("Other UI")]
    [SerializeField] UnitSkillUI _unitSkillUI;
    [SerializeField] UnitStatusUI _unitStatusUI;
    [SerializeField] UnitSellOrAutoSelectionUI _unitSellOrAutoSelectionUI;

    public void Setup(UnitStatus status, bool isUI, bool isSell, bool isEnemy)
    {
        _unitNameText.text = status.Data.Name;
        _unitIcon.sprite = status.Data.Icon;

        if(status.Data.UpgradeData != null)
            _levelText.text = status.Data.UpgradeData.CurrentUpgradeData.UpgradeLevel.ToString();
        else
            _levelText.text = "0";

        _powerText.text = status.CombatPower.ToString();

        // isEnemy로 분리

        _synergyImage.sprite = Manager.Data.SynergyDB.GetSynergy((int)status.Data.Synergy).Icon;
        _classImage.sprite = Manager.Data.SynergyDB.GetSynergy((int)status.Data.ClassSynergy).Icon;

        UnitStats stat = status.GetCurrentStat();

        _hpSlider.maxValue = stat.MaxHealth;
        _hpSlider.value = stat.MaxHealth;
        _hpText.text = $"{stat.MaxHealth}/{stat.MaxHealth}";

        _mpSlider.maxValue = stat.MaxMana;
        _mpSlider.value = stat.MaxMana;
        _mpText.text = $"{stat.MaxMana}/{stat.MaxMana}";

        _unitSkillUI.Setup(status);
        _unitStatusUI.Setup(stat);

        if(isSell)
        {
            _unitSellOrAutoSelectionUI.Setup(status, isUI, Close);
        }
        else
        {
            _unitSellOrAutoSelectionUI.gameObject.SetActive(false);
        }
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}
