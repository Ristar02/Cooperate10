using UnityEngine;

public class HpMeterPresenter
{
    private UnitBase[] _models;
    private HpMeterBar _view;

    public HpMeterPresenter(UnitBase[] models, HpMeterBar view)
    {        
        _models = models;
        _view = view;

        SubscribeToModelEvents();
    }

    private void SubscribeToModelEvents()
    {
        foreach (var unit in _models)
        {
            if(unit != null)
                unit.StatusController.CurHp.AddEvent(_view.Reflash);
        }
    }

    public void Dispose()
    {
        foreach (var unit in _models)
        {
            if (unit != null)
                unit.StatusController.CurHp.RemoveEvent(_view.Reflash);
        }
    }
}