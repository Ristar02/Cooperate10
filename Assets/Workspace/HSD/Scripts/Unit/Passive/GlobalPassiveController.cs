using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalPassiveController
{
    private Vector3 _center;
    private Dictionary<string, GlobalPassive> _passives = new Dictionary<string, GlobalPassive>(128);

    public GlobalPassiveController(Vector3 center)
    {
        _center = center;
    }

    public void AddPassiveEffect(SynergyEffect effect, UnitBase[] units, int multiplier = 1, bool isChange = false, float delay = 0)
    {
        Debug.Log($"[AddPassiveEffect] Effect: {effect.Key}, DelayTime: {effect.DelayTime}, Passed Delay: {delay}, TriggerType: {effect.TriggerType}");
        if (isChange)
        {
            RemovePassiveEffect(effect);
        }

        if (!_passives.ContainsKey(effect.Key))
        {
            _passives.Add(effect.Key, new GlobalPassive(effect, units, _center, multiplier, delay));
            _passives[effect.Key].Active();
        }
    }

    public void RemovePassiveEffect(SynergyEffect effect)
    {
        if (_passives.ContainsKey(effect.Key))
        {
            _passives[effect.Key].Deactive();
            _passives.Remove(effect.Key);
        }
    }

    public void PassiveAllDeActive()
    {
        foreach (var passive in _passives.Values)
        {
            passive.Deactive();
        }
    }

    public void PassiveAllReActive()
    {
        foreach (var passive in _passives.Values)
        {
            passive.Active();
        }
    }
}
