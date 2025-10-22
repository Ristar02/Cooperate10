using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    #region Components
    public SynergyController SynergyController;
    public UI_UnitSlotController UISlotController;

    [SerializeField] UnitSlotManager _unitSlotManager;
    [SerializeField] UnitDragDropSystem _unitDragDropSystem;
    [SerializeField] BattleUnitManager _battleUnitManager;
    #endregion

    public Transform BattleParent;

    #region Unit
    private UnitBase[,] _unitGrid;
    private Dictionary<string, List<UnitBase>> _unitBaseDic = new Dictionary<string, List<UnitBase>>(64);
    private Dictionary<UnitData, int> _unitCountDic = new Dictionary<UnitData, int>(36);
    private Dictionary<int, List<UnitBase>> _synergyUnitDic = new Dictionary<int, List<UnitBase>>(64);
    #endregion

    #region Data
    public static readonly int UnitMaxCount = 10;
    private int _currentUnitCount = 0;
    public int CurrentUnitCount
    {
        get => _currentUnitCount;
        private set
        {
            if (_currentUnitCount != value)
                _currentUnitCount = value;

            OnUnitCountChanged?.Invoke(_currentUnitCount);
        }
    }
    #endregion

    #region Events
    public event System.Action<int> OnUnitCountChanged;
    public event System.Action<int> OnUnitPowerChanged;
    public event System.Action<UnitBase[]> OnUnitChanged;
    #endregion

    public void Init()
    {
        int rows = _unitSlotManager.SlotCreater.Size.y;
        int cols = _unitSlotManager.SlotCreater.Size.x;
        _unitGrid = new UnitBase[rows, cols];

        SynergyController.Init();
        _unitSlotManager.Init();
        _unitDragDropSystem.OnUnitDropped += AddUnit;
        SynergyController.OnSynergyChanged += CheckSynergy;
    }

    #region UnitSetting
    public void UnitStandbyAndSetSlotPosition()
    {
        _unitSlotManager.SlotCreater.ActiveSlots();
        _battleUnitManager._unitSlotManager.SlotCreater.ActiveSlots();

        foreach (var unit in _battleUnitManager.GetUnitGrid())
        {
            if (unit == null)
                continue;

            unit.transform.position = _unitSlotManager.GetUnitSlot(unit.CurrentSlot).transform.position;
            unit.Standby();
            unit.StatusController.Refresh();
            _battleUnitManager._unitSlotManager.SetUnitSlot(unit);
        }        

        _unitDragDropSystem.enabled = true;
    }

    public void UnitsGameEndedStandby()
    {
        foreach (var unit in _battleUnitManager.GetUnitGrid())
        {
            if (unit == null)
                continue;

            unit.GameEndedStanby();
        }
    }

    public void UnitFight()
    {        
        foreach (var unit in _battleUnitManager.GetUnits())
        {
            if (unit == null)
                continue;            

            unit.Fight();
        }
    }

    public void UnitMove()
    {
        foreach (var unit in _battleUnitManager.GetUnitGrid())
        {
            if (unit == null)
                continue;

            unit.Move();
        }

        _unitDragDropSystem.enabled = false;
    }    

    public void UnitIdle()
    {
        foreach (var unit in _battleUnitManager.GetUnitGrid())
        {
            if (unit == null)
                continue;
            
            unit.Idle();
        }
    }

    public void SlotsDeActive()
    {
        foreach (var unit in _battleUnitManager.GetUnitGrid())
        {
            if (unit == null)
                continue;

            unit.transform.SetParent(BattleParent);
        }

        _battleUnitManager._unitSlotManager.SlotCreater.DeActiveSlots();
        _unitSlotManager.SlotCreater.DeActiveSlots();
    }
    #endregion

    #region AddUnit
    /// <summary>
    /// 유닛을 슬롯에 추가합니다.
    /// </summary>    
    public void AddUnit(UnitSlot slot, UnitBase unit)
    {
        UnitBase slotUnit = slot.Unit;

        bool isMax = unit.CurrentSlot != Vector2Int.zero ? false : IsUnitMaxCount() && slotUnit == null;

        if (isMax)
        {
            if (unit.CurrentSlot == Vector2Int.zero)
            {
                UISlotController.SetSlot(unit.Status, _unitDragDropSystem.GetCurrentSlotIdx());
                Destroy(unit.gameObject);
            }
            else
            {
                SetSlot(_unitSlotManager.GetUnitSlot(unit), unit);
                _battleUnitManager.AddUnit(_unitSlotManager.GetUnitSlot(unit), unit);
            }
            return;
        }

        if (unit.CurrentSlot == Vector2Int.zero)
        {
            if (slotUnit != null)
            {
                // 기존 유닛을 UI 슬롯으로 돌려보내기
                UISlotController.SetSlot(slotUnit.Status, _unitDragDropSystem.GetCurrentSlotIdx());

                RemoveUnit(slot, destroyGameObject: false); // 유닛 데이터만 제거 (Destroy 안 함)
                Destroy(slotUnit.gameObject); // UI로 복제했으니 인게임 오브젝트 제거
                _battleUnitManager.RemoveUnit(slot, slotUnit);
            }


            SetSlot(slot, unit);    // 유닛 위치 설정
            _battleUnitManager.AddUnit(slot, unit); // 배틀유닛추가
            AddUnitSynergy(unit);   // 유닛 시너지 추가 및 시너지 체크
            AddUnitCount(unit);     // 유닛 카운트 증가 
            AddList(unit);          // 유닛 리스트에 추가

            CurrentUnitCount++;

            OnUnitPowerChanged?.Invoke(unit.Status.CombatPower);
            OnUnitChanged?.Invoke(GetUnits());
        }
        else
        {
            UnitSlot unitSlot = _unitSlotManager.GetUnitSlot(unit);

            // 스왑
            if (slotUnit != null && slotUnit != unit)
            {
                ClearSlot(unitSlot, unit);
                ClearSlot(slot, slotUnit);

                _battleUnitManager.RemoveUnit(unitSlot, unit);
                _battleUnitManager.RemoveUnit(slot, slotUnit);

                SetSlot(slot, unit);
                SetSlot(unitSlot, slotUnit);

                _battleUnitManager.AddUnit(slot, unit);
                _battleUnitManager.AddUnit(unitSlot, slotUnit);
            }
            else
            {
                ClearSlot(unitSlot, unit);

                _battleUnitManager.MoveUnit(unitSlot, slot, unit);
                SetSlot(slot, unit);
            }
        }

        if (unit != null)
            CheckSynergy(unit);
        if (slotUnit != null)
            CheckSynergy(slotUnit);
    }

    public void AddUnit(UnitBase newUnit, Vector2Int pos)
    {
        UnitSlot slot = _unitSlotManager.GetUnitSlot(pos);

        newUnit.transform.SetParent(slot.transform);
        newUnit.transform.position = slot.transform.position;
        newUnit.Init();

        AddUnit(slot, newUnit);
    }

    public void AddUnit(UnitStatus newUnitData, Vector2Int pos)
    {
        GameObject obj = Instantiate(newUnitData.Data.UnitPrefab);
        UnitSlot slot = _unitSlotManager.GetUnitSlot(pos);        
        UnitBase newUnit = ComponentProvider.Get<UnitBase>(obj);
        newUnit.Status = newUnitData;

        newUnit.transform.SetParent(slot.transform);
        newUnit.transform.position = slot.transform.position;
        newUnit.Init();

        AddUnit(slot, newUnit);
    }
    #endregion

    #region RemoveUnit
    public void RemoveUnit(UnitBase unit)
    {
        UnitSlot slot = GetUnitSlot(unit);

        RemoveUnit(slot);
    }

    public void RemoveUnit(UnitSlot slot, bool destroyGameObject = true)
    {
        if (slot.Unit == null) return;

        UnitBase unit = slot.Unit;

        _unitGrid[unit.CurrentSlot.y - 1, unit.CurrentSlot.x - 1] = null;
        _unitBaseDic[unit.Status.Address].Remove(unit);

        RemoveUnitCount(unit);
        RemoveSynergy(unit);

        ClearSlot(slot, unit);

        if (destroyGameObject)
        {
            Destroy(unit.gameObject);
            _battleUnitManager.RemoveUnit(slot, unit);
        }

        CurrentUnitCount--;

        OnUnitPowerChanged?.Invoke(-unit.Status.CombatPower);
        OnUnitChanged?.Invoke(GetUnits());
    }

    public Vector2Int RemoveUnitGetPosition(UnitStatus unit)
    {
        if (unit == null || string.IsNullOrEmpty(unit.Address))
        {
            return Vector2Int.zero;
        }

        if (!_unitBaseDic.TryGetValue(unit.Address, out var unitList) || unitList.Count == 0)
        {
            return Vector2Int.zero;
        }

        UnitBase unitBase = unitList[0];
        Vector2Int pos = unitBase.CurrentSlot;

        UnitSlot slot = _unitSlotManager.GetUnitSlot(unitBase);
        RemoveUnit(slot);

        return pos;
    }


    public void RemoveUnit(UnitStatus removeUnit)
    {
        foreach (var unit in GetUnits())
        {
            if(unit != null && unit.Status == removeUnit)
            {
                RemoveUnit(unit);
                return;
            }
        }
    }
    #endregion

    #region Slot
    private void SetSlot(UnitSlot slot, UnitBase unit)
    {
        slot.SetUnit(unit);
        _unitGrid[unit.CurrentSlot.y - 1, unit.CurrentSlot.x - 1] = unit;
    }

    private void ClearSlot(UnitSlot slot, UnitBase unit)
    {
        slot.ClearSlot();

        _unitGrid[unit.CurrentSlot.y - 1, unit.CurrentSlot.x - 1] = null;
    }
    #endregion

    private void AddList(UnitBase unit)
    {
        if (!_unitBaseDic.TryGetValue(unit.Status.Address, out var list))
        {
            list = new List<UnitBase>(4);
            _unitBaseDic.Add(unit.Status.Address, list);
        }
        list.Add(unit);
    }

    #region UnitCount
    private void AddUnitCount(UnitBase unit)
    {
        if (!_unitCountDic.ContainsKey(unit.Status.Data))
            _unitCountDic.Add(unit.Status.Data, 0);

        _unitCountDic[unit.Status.Data]++;
    }

    private void RemoveUnitCount(UnitBase unit)
    {
        if (_unitCountDic.ContainsKey(unit.Status.Data))
            _unitCountDic[unit.Status.Data]--;
    }
    #endregion

    #region Synergy
    private void AddUnitSynergy(UnitBase unit)
    {
        if (_unitCountDic.ContainsKey(unit.Status.Data) && _unitCountDic[unit.Status.Data] >= 1)
        {
            return;
        }

        Synergy synergy = unit.Status.Data.Synergy;
        ClassType classSynergy = unit.Status.Data.ClassSynergy;

        SynergyController.AddSynergy(synergy, classSynergy);

        AddSynergyUnit(unit, (int)synergy);
        AddSynergyUnit(unit, (int)classSynergy);
    }

    private void RemoveSynergy(UnitBase unit)
    {
        if (_unitCountDic[unit.Status.Data] >= 1)
        {
            return;
        }

        Synergy synergy = unit.Status.Data.Synergy;
        ClassType classSynergy = unit.Status.Data.ClassSynergy;

        SynergyController.RemoveSynergy(synergy, classSynergy);

        RemoveSynergyUnit(unit, (int)synergy);
        RemoveSynergyUnit(unit, (int)classSynergy);
    }

    private void AddSynergyUnit(UnitBase unit, int synergy)
    {
        if (!_synergyUnitDic.TryGetValue(synergy, out var synergyList))
        {
            synergyList = new List<UnitBase>(16);
            _synergyUnitDic[synergy] = synergyList;
        }
        synergyList.Add(unit);
    }

    private void RemoveSynergyUnit(UnitBase unit, int synergy)
    {
        if (_synergyUnitDic.TryGetValue(synergy, out var synergyList))
            synergyList.Remove(unit);
    }

    private void CheckSynergy(int synergyIdx, int synergyCount)
    {
        SynergyData synergy = Manager.Data.SynergyDB.GetSynergy(synergyIdx);

        if (synergy == null) return;

        synergy.Check(synergyCount, GetUnits());
    }

    private void CheckSynergy(UnitBase unit)
    {
        SynergyData synergy = Manager.Data.SynergyDB.GetSynergy((int)unit.Status.Data.Synergy);
        SynergyData classSynergy = Manager.Data.SynergyDB.GetSynergy((int)unit.Status.Data.ClassSynergy);

        if (synergy == null) return;

        synergy.Check(SynergyController.GetSynergyUnitCount((int)unit.Status.Data.Synergy), GetUnits());
        classSynergy.Check(SynergyController.GetSynergyUnitCount((int)unit.Status.Data.ClassSynergy), GetUnits());
    }

    #endregion

    #region Gettters
    public UnitSlot GetUnitSlot(UnitBase unit)
    {
        return _unitSlotManager.UnitSlotDic[unit.CurrentSlot];
    }

    public UnitSlot GetUnitSlot(Vector2Int pos)
    {
        return _unitSlotManager.GetUnitSlot(pos);
    }

    public int GetUnitCount(string address)
    {
        return _unitBaseDic.TryGetValue(address, out var unitList) ? unitList.Count : 0;
    }

    public int GetUnitCount(UnitStatus unit)
    {
        return GetUnitCount(unit.Address);
    }
    /// <summary>
    /// GC 할당 없이 현재 유닛들을 반환합니다.
    /// 반환된 배열의 유효한 요소는 처음부터 GetUnitsCount()개까지입니다.
    /// </summary>
    public UnitBase[] GetUnits()
    {
        return _battleUnitManager.GetUnits();
    }

    /// <summary>
    /// GetUnits()로 반환된 배열에서 유효한 유닛의 개수를 반환합니다.
    /// </summary>
    public int GetUnitsCount()
    {
        return _battleUnitManager.GetUnitsCount();
    }

    /// <summary>
    /// 특정 인덱스의 유닛을 반환합니다. (범위 체크 포함)
    /// </summary>
    public UnitBase GetUnit(int index)
    {
        return _battleUnitManager.GetUnit(index);
    }

    public bool IsUnitMaxCount()
    {
        return _currentUnitCount >= UnitMaxCount;
    }

    public UnitSlot GetEmptyLineSlot(int line)
    {
        for (int y = 1; y <= _unitSlotManager.SlotCreater.Size.y; y++)
        {
            Vector2Int pos = new Vector2Int(line, y);

            if (_unitGrid[y - 1, line - 1] == null)
            {
                return _unitSlotManager.GetUnitSlot(pos);
            }
        }

        return null;
    }
    #endregion
}