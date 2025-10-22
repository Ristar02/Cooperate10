using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public List<UnitData> ownedUnits = new List<UnitData>();
    // 또는 Dictionary<int, UnitData> ownedUnitsById;
}