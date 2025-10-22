using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnitGridData", menuName = "Data/UnitGridData")]
public class UnitGridDataSO : ScriptableObject
{
    public string gridName;
    public List<GridUnitData> unitDatas = new List<GridUnitData>();

    // 기존 호환성을 위한 메서드 (UnitData 반환)
    public UnitData GetUnitData(Vector2Int position)
    {
        var unitInfo = unitDatas.Find(u => u.position == position);
        return unitInfo?.unitStatus.Data;
    }

    // 새로운 UnitStatus 반환 메서드
    public UnitStatus GetUnitStatus(Vector2Int position)
    {
        var unitInfo = unitDatas.Find(u => u.position == position);
        return unitInfo?.unitStatus;
    }

    // 기존 SetUnitData 메서드 (레벨 포함)
    public void SetUnitData(Vector2Int position, UnitData unitData, int level)
    {
        var existingUnit = unitDatas.Find(u => u.position == position);
        UnitStatus unitStatus = new UnitStatus(unitData, level);

        if (existingUnit != null)
        {
            existingUnit.unitStatus = unitStatus;
        }
        else
        {
            unitDatas.Add(new GridUnitData(position, unitStatus));
        }
    }

    // 새로운 UnitStatus 설정 메서드
    public void SetUnitStatus(Vector2Int position, UnitStatus unitStatus)
    {
        var existingUnit = unitDatas.Find(u => u.position == position);

        if (existingUnit != null)
        {
            existingUnit.unitStatus = unitStatus;
        }
        else
        {
            unitDatas.Add(new GridUnitData(position, unitStatus));
        }
    }

    public void RemoveUnitData(Vector2Int position)
    {
        unitDatas.RemoveAll(u => u.position == position);
    }

    public void ClearAllUnitDatas()
    {
        unitDatas.Clear();
    }

    // 디버깅을 위한 메서드
    public void LogAllUnitStatuses()
    {
        Debug.Log($"Grid '{gridName}' contains {unitDatas.Count} units:");
        foreach (var unitData in unitDatas)
        {
            Debug.Log($"Position: {unitData.position}, Unit: {unitData.unitStatus.Data.Name}, Level: {unitData.unitStatus.Level + 1}");
        }
    }
}