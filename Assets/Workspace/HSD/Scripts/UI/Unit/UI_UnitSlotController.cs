using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UI_UnitSlotController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] UnitDragDropSystem _dragDropSystem;
    [SerializeField] UnitController _unitController;

    [Header("UI Elements")]
    [SerializeField] Transform _content;
    [SerializeField] GridLayoutGroup _gridLayoutGroup;
    [SerializeField] int _slotCount;
    [SerializeField] int _offset;

    [SerializeField] UI_UnitSlot[] _unitSlots;
    private UnitStatus[] _cachedUnitsArray = new UnitStatus[UnitController.UnitMaxCount];
    private Dictionary<string, List<int>> _unitSlotDic = new Dictionary<string, List<int>>(10);

    private void Start()
    {
        CreateSlots();
        UI_UnitSlot.OnUnitChanged += SetSlot;
    }

    private void OnDestroy()
    {
        UI_UnitSlot.OnUnitChanged -= SetSlot;
    }

    private void CreateSlots()
    {        
        _gridLayoutGroup.SetupGridLayoutGroup(_content, 5, 2, _offset, true);

        for (int i = 0; i < _slotCount; i++)
        {
            _unitSlots[i].ClearSlot();
            _unitSlots[i].Init(_dragDropSystem, i, this);
        }
    }

    public void SetSlot(UnitStatus unit, int idx)
    {
        _unitSlots[idx].SetSlot(unit);
        AddUnit(unit, idx);
    }

    public void ClearSlot(int idx)
    {
        UnitStatus unit = _unitSlots[idx].GetUnit();
        if (unit == null) return;

        _unitSlotDic[unit.Address].Remove(idx);

        _unitSlots[idx].ClearSlot();

        RemoveCahchedArray(idx);
    }

    public void ClearSlot(UnitStatus unit)
    {
        GetUnitSlot(unit)?.ClearSlot();
    }

    /// <summary>
    /// 빈 슬롯의 인덱스를 반환합니다. 없으면 -1 반환
    /// </summary>
    /// <returns></returns>
    public int GetEmptySlot()
    {
        for (int i = 0; i < _unitSlots.Length; i++)
        {
            if (_unitSlots[i].IsEmpty())
            {
                return i;
            }
        }
        return -1;
    }

    public void AddEmptySlotUnit(UnitStatus status)
    {
        int emptyIdx = GetEmptySlot();

        if (emptyIdx == -1)
        {
            Debug.LogWarning("No empty slots available!");
            return;
        }

        SetSlot(status, emptyIdx);
    }

    public void AddUnit(UnitStatus unit, int idx)
    {
        if (!_unitSlotDic.ContainsKey(unit.Address))
        {
            _unitSlotDic.Add(unit.Address, new List<int>(5));
        }

        var slotList = _unitSlotDic[unit.Address];

        if (!slotList.Contains(idx))
        {
            slotList.Add(idx);
        }

        AddCahchedArray(unit, idx);
    }

    public void RemoveUnit(UnitStatus unit, int idx)
    {
        if (_unitSlotDic.ContainsKey(unit.Address))
        {
            _unitSlotDic[unit.Address].Remove(idx);
        }     
    }

    public void RemoveLastUnit(UnitStatus unit)
    {
        var slotList = _unitSlotDic[unit.Address];
        int lastIdx = slotList[slotList.Count - 1];
        ClearSlot(lastIdx);
    }

    /// <summary>
    /// 인 게임 슬롯에서 유닛을 제거합니다.
    /// </summary>    
    public void RemoveInGameSlot(UnitBase unit, int slotIdx, bool isSwitch = false)
    {
        if (unit == null || unit.StatusController == null)
        {
            Debug.LogWarning("RemoveInGameSlot called with invalid unit");
            return;
        }

        if (unit.CurrentSlot != Vector2Int.zero)
        {
            UnitSlot slot = _unitController.GetUnitSlot(unit);
            _unitController.RemoveUnit(slot); // 슬롯에 있는 유닛 삭제
        }

        if(!isSwitch)
            RemoveUnit(unit.StatusController.Status, slotIdx);        
    }

    public void ReturnUnitToUI(UnitBase unit)
    {
        int emptySlotIdx = GetEmptySlot();
        if (emptySlotIdx == -1)
        {
            Debug.LogWarning("No empty UI slots available!");
            return;
        }

        SetSlot(unit.Status, emptySlotIdx);
    }

    public void AddInGameSlot(UnitStatus unit, int slotIdx, Vector2Int pos)
    {
        UnitSlot slot = _unitController.GetUnitSlot(pos);
        _unitController.AddUnit(unit, pos);
    }

    private UI_UnitSlot GetUnitSlot(UnitStatus unit)
    {
        if (_unitSlotDic.TryGetValue(unit.Address, out List<int> slotIdxs))
        {
            foreach (int idx in slotIdxs)
            {
                if (_cachedUnitsArray[idx] == unit)
                {
                    return _unitSlots[idx];
                }
            }
        }
        return null;
    }
    public int GetUnitCount(string address)
    {
        if (_unitSlotDic.TryGetValue(address, out List<int> slotIdxs))
        {
            return slotIdxs.Count;
        }
        return 0;
    }

    public int GetUnitCount(UnitStatus unit)
    {
        return GetUnitCount(unit.Address);
    }

    private void AddCahchedArray(UnitStatus unit, int idx)
    {
        _cachedUnitsArray[idx] = unit;
    }

    private void RemoveCahchedArray(int idx)
    {
        _cachedUnitsArray[idx] = null;
    }

    public UnitStatus[] GetUnits()
    {
        return _cachedUnitsArray;
    }

    public UI_UnitSlot[] GetUnitSlots()
    {
        return _unitSlots;
    }
}
