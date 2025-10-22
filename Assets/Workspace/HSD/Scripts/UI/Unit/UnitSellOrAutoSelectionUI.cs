using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UnitSellOrAutoSelectionUI : UIBase
{
    [Header("Component")]
    [SerializeField] UnitManager _unitManager;

    [UIBind("SellButton")] Button _sellButton;
    [UIBind("AutoButton")] Button _autoSelectionButton;
    [UIBind("Amount")] TMP_Text _sellAmountText;

    private int _sellGold;
    private UnitStatus _currentUnit;

    public void Setup(UnitStatus unit, bool isUI, UnityAction Close)
    {
        gameObject.SetActive(true);

        _currentUnit = unit;
        _sellGold = (unit.Level + 1) * 20;

        _sellAmountText.text = Utils.ToAbbreviation(_sellGold);

        _sellButton.onClick.RemoveAllListeners();
        _autoSelectionButton.onClick.RemoveAllListeners();

        if (isUI)
        {
            _sellButton.onClick.AddListener(SellUI);
        }
        else
        {
            _sellButton.onClick.AddListener(SellBattleUnit);
        }

        _autoSelectionButton.onClick.AddListener(AutoSelection);

        _sellButton.onClick.AddListener(Close);
        _autoSelectionButton.onClick.AddListener(Close);
    }

    private void SellBattleUnit()
    {
        InGameManager.Instance.AddGold(_sellGold);
        _unitManager.UnitController.RemoveUnit(_currentUnit);
    }

    private void SellUI()
    {
        InGameManager.Instance.AddGold(_sellGold);
        _unitManager.UnitController.UISlotController.ClearSlot(_currentUnit);
    }

    private void AutoSelection()
    {
        int perferredLine = _currentUnit.Data.PerferredLine;
        UnitSlot slot = _unitManager.UnitController.GetEmptyLineSlot(perferredLine);
        _unitManager.UnitController.UISlotController.ClearSlot(_currentUnit);

        if (slot != null)
        {
            _unitManager.AddBattleUnit(_currentUnit, slot);
        }        
    }
}
