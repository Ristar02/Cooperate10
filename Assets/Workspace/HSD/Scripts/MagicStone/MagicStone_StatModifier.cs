using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MagicStone_StatModifierData", menuName = "Data/MagicStoneData/StatModifier")]
public class MagicStone_StatModifier : MagicStoneData
{
    [SerializeField] StatType _statType;

    public override void UseMagicStone(Vector2 pos)
    {
        SpawnEffect(pos);

        ApplyStatMultiple(GetMultipleTargets(pos), _statType);
    }
}
