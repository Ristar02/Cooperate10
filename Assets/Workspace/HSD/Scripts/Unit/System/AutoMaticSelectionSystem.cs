using System;
using System.Collections.Generic;
using UnityEngine;

public class AutoMaticSelectionSystem : MonoBehaviour
{
    [Serializable]
    public class AutoUnitInfo
    {
        public UnitStatus Status;
        public UI_UnitSlot UI_UnitSlot;
        public AutoUnitType AutoUnitType;

        public AutoUnitInfo(AutoUnitType autoUnitType, UnitStatus status = null, UI_UnitSlot unitSlot = null)
        {
            Status = status;
            UI_UnitSlot = unitSlot;
            AutoUnitType = autoUnitType;
        }
    }

    [SerializeField] private UnitManager _unitManager;
    private UI_UnitSlot[] _uiUnitSlots => _unitManager.UnitController.UISlotController.GetUnitSlots();
    private UnitBase[] _battleUnits => _unitManager.UnitController.GetUnits();

    private readonly List<AutoUnitInfo> _units = new List<AutoUnitInfo>(20);

    public void AutoSelectCharacters()
    {
#if UNITY_EDITOR
        TestUtils.TimerStart();
#endif
        _units.Clear();

        foreach (var battleUnit in _battleUnits)
        {
            if (battleUnit?.Status?.Data != null)
            {
                _units.Add(new AutoUnitInfo(AutoUnitType.Unit, status: battleUnit.Status));
            }
        }

        foreach (var uiUnitSlot in _uiUnitSlots)
        {
            var unit = uiUnitSlot.GetUnit();
            if (unit?.Data != null)
            {
                _units.Add(new AutoUnitInfo(AutoUnitType.Slot, unit, uiUnitSlot));
            }
        }

        foreach (var info in _units)
        {
            if (info.AutoUnitType == AutoUnitType.Unit)
            {
                _unitManager.UnitController.RemoveUnitGetPosition(info.Status);
            }
            else
            {
                _unitManager.UnitController.UISlotController.ClearSlot(info.UI_UnitSlot.GetSlotIdx());
            }
        }

        _units.Sort((a, b) => b.Status.CombatPower.CompareTo(a.Status.CombatPower));

        int battleCount = Mathf.Min(10, _units.Count);
        for (int i = 0; i < battleCount; i++)
        {
            SetBattleUnit(_units[i].Status);
        }

        for (int i = battleCount; i < _units.Count; i++)
        {
            _unitManager.UnitController.UISlotController.AddEmptySlotUnit(_units[i].Status);
        }

#if UNITY_EDITOR
        TestUtils.TimerStop();
#endif
    }

    private void SetBattleUnit(UnitStatus status)
    {
        int preferredLine = status.Data.PerferredLine;
        UnitSlot slot = _unitManager.UnitController.GetEmptyLineSlot(preferredLine);

        if (slot == null)
        {
            for (int offset = 1; offset <= 4 && slot == null; offset++)
            {
                int right = preferredLine + offset;
                int left = preferredLine - offset;

                if (right <= 5)
                    slot = _unitManager.UnitController.GetEmptyLineSlot(right);

                if (slot == null && left >= 1)
                    slot = _unitManager.UnitController.GetEmptyLineSlot(left);
            }
        }

        if (slot != null)
        {
            _unitManager.AddBattleUnit(status, slot);
        }
    }
}
