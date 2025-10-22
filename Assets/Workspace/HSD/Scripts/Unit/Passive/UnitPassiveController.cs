using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UnitPassiveController
{
    private UnitBase _owner;
    private Dictionary<string, UnitPassive> _passives = new Dictionary<string, UnitPassive>(128);

    public UnitPassiveController(GameObject owner)
    {
        _owner = ComponentProvider.Get<UnitBase>(owner);
    }

    public void AddPassiveEffect(SynergyEffect effect, int statMultiplier = 1, bool isChange = false)
    {
        if (isChange)
        {
            RemovePassiveEffect(effect);
        }

        if (!_passives.ContainsKey(effect.Key))
        {
            _passives.Add(effect.Key, new UnitPassive(effect, _owner, statMultiplier));
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

    public void DeActiveAllPassive()
    {
        PassiveAllDeActive();
        _passives.Clear();
    }

    public void PassiveAllDeActive()
    {
        foreach (var passive in _passives.Values)
        {
            passive.Deactive();
        }
    }

    public void RefreshBaseStats()
    {
        foreach (var passive in _passives.Values)
        {
            if(passive.Effect.TriggerType != TriggerType.Base)
            
            passive.Active();
        }
    }
}
