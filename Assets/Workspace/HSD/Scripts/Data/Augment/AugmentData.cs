using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AugmentData", menuName = "Data/Augment")]
public class AugmentData : MetaData
{
    private readonly string _key = System.Guid.NewGuid().ToString();
    [SerializeField] StatEffectModifier[] _statEffectModifiers;

    public void Apply()
    {
        foreach (var modifier in _statEffectModifiers)
        {
            //AugmentManager.Instance.AddAugment(modifier.StatType, modifier.Value, _key);
        }
    }
}
