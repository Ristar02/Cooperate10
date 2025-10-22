using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] UnitGridDataSO _gridDataSO;
    [SerializeField] UnitSlotManager _slotManager;
    [SerializeField] LayerMask _targetLayer;
    private UnitBase[,] _unitGrid;

    [Header("Parent")]
    public Transform BattleParent;

    public void Init()
    {
        _slotManager.Init();
        _unitGrid = new UnitBase[_slotManager.SlotCreater.Size.y, _slotManager.SlotCreater.Size.x];        
    }

    public void SetUnit(UnitGridDataSO gridData)
    {
        _gridDataSO = gridData;

        foreach (var unitData in _gridDataSO.unitDatas)
        {
            UnitSlot slot = _slotManager.GetUnitSlot(unitData.position + new Vector2Int(1, 1));

            int x = unitData.position.x;
            int y = unitData.position.y;

            GameObject obj = Instantiate(unitData.unitStatus.Data.UnitPrefab);
            UnitBase unit = ComponentProvider.Get<UnitBase>(obj);

            if(unitData.unitStatus == null)
                Debug.Log($"UnitStatus is null at position {unitData.position}");

            unit.Status = unitData.unitStatus;

            unit.transform.position = slot.transform.position;
            unit.transform.SetParent(slot.transform);
            unit.TargetLayer = _targetLayer;
            unit.gameObject.layer = LayerMask.NameToLayer("Enemy");
            unit.Init();

            unit.SetBattleUnit(-slot.GetPos().y + 5);

            if (unit.transform.localScale.x < 0)
                unit.transform.localScale = new Vector3(-unit.transform.localScale.x, unit.transform.localScale.y, unit.transform.localScale.z);

            _unitGrid[y, x] = unit;
        }
    }

    public void EnemyFight()
    {
        foreach (var unit in _unitGrid)
        {
            if (unit == null)
                continue;

            unit.Fight();
        }        
    }

    public void EnemyMove()
    {
        foreach (var unit in _unitGrid)
        {
            if (unit == null)
                continue;

            unit.Move();
        }
    }

    public void EnemyIdle()
    {
        foreach (var unit in _unitGrid)
        {
            if (unit == null)
                continue;

            unit.Idle();
        }
    }

    public void EnemyStandby()
    {
        foreach (var unit in _unitGrid)
        {
            if (unit == null || unit.StatusController.IsDead)
                continue;

            unit.Standby();
        }
    }
    public void SlotsDeActive()
    {
        foreach (var unit in _unitGrid)
        {
            if (unit == null)
                continue;

            unit.transform.SetParent(BattleParent);
        }

        _slotManager.SlotCreater.DeActiveSlots();
    }

    public void ResetEnemy()
    {
        ClearAllEnemyUnits();
        _slotManager.SlotCreater.ActiveSlots();
    }

    private void ClearAllEnemyUnits()
    {
        if (_unitGrid == null)
            return;

        foreach (var unit in _unitGrid)
        {
            if (unit != null)
            {
                UnitSlot slot = _slotManager.GetUnitSlot(unit);

                if (slot == null)
                    return;

                slot.ClearSlot();

                Destroy(unit.gameObject);
            }
        }
    }

    public UnitBase[] GetUnits()
    {
        List<UnitBase> units = new List<UnitBase>();

        foreach (var unit in _unitGrid)
        {
            if (unit != null)
                units.Add(unit);
        }

        return units.ToArray();
    }
}
