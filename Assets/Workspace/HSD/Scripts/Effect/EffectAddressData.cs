using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectAddressData", menuName = "Data/EffectAddressData")]
public class EffectAddressData : ScriptableObject
{
    [SerializeField] private BuffEffectAddress[] _buffEffectAddresses;

    private Dictionary<int, string> _addressDic;

    private void BuildDictionary()
    {
        _addressDic = new Dictionary<int, string>();
        foreach (var buffEffectAddress in _buffEffectAddresses)
        {
            if (!_addressDic.ContainsKey((int)buffEffectAddress.BuffEffect))
            {
                _addressDic.Add((int)buffEffectAddress.BuffEffect, buffEffectAddress.Address);
            }
            else
            {
                Debug.LogWarning($"중복된 BuffEffect 발견: {buffEffectAddress.BuffEffect}");
            }
        }
    }

    public string GetBuffAddress(BuffEffect buffEffect)
    {
        if (_addressDic == null || _addressDic.Count == 0)
            BuildDictionary();

        if (_addressDic.TryGetValue((int)buffEffect, out var address))
            return address;

        Debug.LogError($"BuffEffect {buffEffect} 에 해당하는 주소가 없음!");
        return null;
    }
}

[System.Serializable]
public class BuffEffectAddress
{
    public BuffEffect BuffEffect;
    public string Address;
}