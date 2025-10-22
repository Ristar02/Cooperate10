using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageMeterPresenter
{
    private readonly IDamageMeterView _view;
    private readonly UnitStatusController _status;

    public DamageMeterPresenter(IDamageMeterView view, UnitStatusController status)
    {
        _view = view;
        _status = status;
        
        _view.SetIcon(_status.Status.Data.Icon);
        _view.SetDamage(_status.TotalDamage.Value);

        _status.TotalDamage.AddEvent(OnDamageChanged);
    }

    private void OnDamageChanged(int damage)
    {
        _view.SetDamage(damage);
    }

    public void Dispose()
    {
        _status.TotalDamage.RemoveEvent(OnDamageChanged);
    }
}
