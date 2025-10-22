using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSlot : MonoBehaviour
{
    public UnitBase Unit;

    private SpriteRenderer _sr;
    private int _line;
    private Vector2Int _pos;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();        
    }

    public void Init(int line, Vector2Int pos)
    {
        _line = line;
        _pos = pos;
    }  

    public void SetUnit(UnitBase unit)
    {
        if (unit == null)
        {
            ClearSlot();
            return;
        }

        unit.Idle();
        unit.CurrentSlot = _pos;
        unit.gameObject.transform.position = transform.position;
        unit.gameObject.transform.SetParent(transform, true);
        Unit = unit;
    }

    public void ClearSlot()
    {
        Unit = null;
    }

    public Vector2Int GetPos()
    {
        return _pos;
    }
}
