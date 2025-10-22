using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BattleUnitManager : MonoBehaviour
{
    public UnitSlotManager _unitSlotManager;
    private UnitBase[,] _unitGrid;
    private int UnitMaxCount => UnitController.UnitMaxCount;
    private UnitBase[] _cachedUnitsArray = new UnitBase[UnitController.UnitMaxCount];
    private int _cachedUnitsCount = 0;

    private void Awake()
    {
        int rows = _unitSlotManager.SlotCreater.Size.y;
        int cols = _unitSlotManager.SlotCreater.Size.x;
        _unitGrid = new UnitBase[rows, cols];

        _unitSlotManager.Init();
    }

    public void MoveUnit(UnitSlot oldSlot, UnitSlot slot, UnitBase unit)
    {
        UnitSlot newOldSlot = _unitSlotManager.GetUnitSlot(oldSlot.GetPos());
        UnitSlot newSlot = _unitSlotManager.GetUnitSlot(slot.GetPos());
        UnitBase unitBase = _unitGrid[unit.CurrentSlot.y - 1, unit.CurrentSlot.x - 1];

        ClearSlot(newOldSlot, unitBase, false);

        SetSlot(newSlot, unitBase);
    }

    public void AddUnit(UnitSlot slot, UnitBase unit)
    {
        UnitSlot newSlot = _unitSlotManager.GetUnitSlot(slot.GetPos());
        UnitBase newUnit = ComponentProvider.Get<UnitBase>(Instantiate(unit.Status.Data.UnitPrefab));
        newUnit.Status = unit.Status;
        AddToCachedArray(newUnit);
        newUnit.Init();
        newUnit.SetBattleUnit(-slot.GetPos().y);

        AugmentManager.Instance.ApplyAugment(newUnit);

        SetSlot(newSlot, newUnit);
    }

    public void RemoveUnit(UnitSlot slot, UnitBase unit)
    {
        UnitSlot newSlot = _unitSlotManager.GetUnitSlot(slot.GetPos());
        UnitBase unitBase = _unitGrid[unit.CurrentSlot.y - 1, unit.CurrentSlot.x - 1];

        RemoveFromCachedArray(unitBase);

        AugmentManager.Instance.ReleaseAugment(unitBase);

        ClearSlot(newSlot, unitBase);
    }

    public void ClearSlot(UnitSlot slot, UnitBase unit, bool isDestroy = true)
    {
        slot.ClearSlot();

        _unitGrid[unit.CurrentSlot.y - 1, unit.CurrentSlot.x - 1] = null;

        if (isDestroy)
            Destroy(unit.gameObject);
    }

    public void SetSlot(UnitSlot slot, UnitBase unit)
    {
        slot.SetUnit(unit);
        _unitGrid[unit.CurrentSlot.y - 1, unit.CurrentSlot.x - 1] = unit;
    }

    public UnitBase[,] GetUnitGrid()
    {
        return _unitGrid;
    }

    private void AddToCachedArray(UnitBase unit)
    {
        if (_cachedUnitsCount < UnitMaxCount)
        {
            _cachedUnitsArray[_cachedUnitsCount] = unit;
            _cachedUnitsCount++;
        }
    }

    private void RemoveFromCachedArray(UnitBase unit)
    {
        for (int i = 0; i < _cachedUnitsCount; i++)
        {
            if (_cachedUnitsArray[i] == unit)
            {
                for (int j = i; j < _cachedUnitsCount - 1; j++)
                {
                    _cachedUnitsArray[j] = _cachedUnitsArray[j + 1];
                }

                _cachedUnitsArray[_cachedUnitsCount - 1] = null;
                _cachedUnitsCount--;
                break;
            }
        }
    }
    /// <summary>
    /// GC 할당 없이 현재 유닛들을 반환합니다.
    /// 반환된 배열의 유효한 요소는 처음부터 GetUnitsCount()개까지입니다.
    /// </summary>
    public UnitBase[] GetUnits()
    {
        return _cachedUnitsArray;
    }

    /// <summary>
    /// GetUnits()로 반환된 배열에서 유효한 유닛의 개수를 반환합니다.
    /// </summary>
    public int GetUnitsCount()
    {
        return _cachedUnitsCount;
    }

    /// <summary>
    /// 특정 인덱스의 유닛을 반환합니다. (범위 체크 포함)
    /// </summary>
    public UnitBase GetUnit(int index)
    {
        if (index < 0 || index >= _cachedUnitsCount)
            return null;

        return _cachedUnitsArray[index];
    }
}