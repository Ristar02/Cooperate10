using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitSynergyData", menuName = "Data/Synergy/Unit")]
public class UnitSynergyData : SynergyData
{
    public Synergy Synergy;

    public override void Init()
    {
        base.Init();
        _synergy = (int)Synergy;
    }
}
