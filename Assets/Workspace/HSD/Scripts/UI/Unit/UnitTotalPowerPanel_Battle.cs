using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTotalPowerPanel_Battle : UnitTotalPowerPanel
{
    public override void UpdateTotalPower(int power)
    {
        _totalPower += power;

        _powerText.text = $"전투력 : {_totalPower.ToString("N0")}";
    }
}
