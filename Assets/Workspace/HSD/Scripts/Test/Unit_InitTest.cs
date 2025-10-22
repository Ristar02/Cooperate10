using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_InitTest : MonoBehaviour
{
    [SerializeField] UnitBase[] _units;
    [SerializeField] UnitData _testData;

    public void Init()
    {
        foreach (var unit in _units)
        {
            unit.Status = new UnitStatus(_testData);
            unit.Init();
        }
    }
}
