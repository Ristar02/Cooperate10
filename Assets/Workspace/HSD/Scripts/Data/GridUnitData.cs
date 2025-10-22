using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridUnitData
{
    public Vector2Int position;
    public UnitStatus unitStatus;

    public GridUnitData(Vector2Int pos, UnitStatus data)
    {
        position = pos;
        unitStatus = data;
    }
}