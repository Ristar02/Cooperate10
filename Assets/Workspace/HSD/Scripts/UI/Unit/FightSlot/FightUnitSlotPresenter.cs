using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightUnitSlotPresenter
{
    private readonly IFightUnitSlotView _view;
    private readonly UnitStatusController _status;

    public FightUnitSlotPresenter(IFightUnitSlotView view, UnitStatusController status)
    {
        _view = view;
        _status = status;

        _view.SetIcon(_status.Status.Data.Icon);
        _view.SetHp(_status.CurHp.Value, _status.MaxHealth.Value);
        _view.SetMana(_status.CurMana.Value, _status.MaxMana.Value);

        _status.CurHp.AddEvent(OnHpChanged);
        _status.CurMana.AddEvent(OnManaChanged);
    }

    private void OnHpChanged(int value)
    {
        _view.SetHp(value, _status.MaxHealth.Value);
    }

    private void OnManaChanged(int value)
    {
        _view.SetMana(value, _status.MaxMana.Value);
    }

    public void Dispose()
    {
        if (_status == null)
        {
            Debug.LogWarning("Status is null, cannot dispose FightUnitSlotPresenter.");
            return;
        }

        _status.CurHp.RemoveEvent(OnHpChanged);
        _status.CurMana.RemoveEvent(OnManaChanged);
    }
}
