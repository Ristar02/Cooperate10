using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSlotManager : MonoBehaviour
{
    public SlotCreater SlotCreater;
    public Dictionary<Vector2Int, UnitSlot> UnitSlotDic = new Dictionary<Vector2Int, UnitSlot>(20);

    public void Init()
    {
        UnitSlotDic = SlotCreater.Init();
    }

    public UnitSlot GetUnitSlot(UnitBase unit)
    {
        return GetUnitSlot(unit.CurrentSlot);
    }

    public UnitSlot GetUnitSlot(Vector2Int pos)
    {
        return UnitSlotDic.TryGetValue(pos, out UnitSlot slot) ? slot : null;
    }

    public void SetUnitSlot(UnitBase unit)
    {
        UnitSlotDic[unit.CurrentSlot].SetUnit(unit);
    }
}
