using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UnitHealthBar : MonoBehaviour
{
    [SerializeField] Slider _slider;
    [SerializeField] ShieldSlider _shieldSlider;
    private UnitBase _owner;    
    private CancellationTokenSource _cts;

    private void OnDisable()
    {
        Dispose();
    }

    public void Setup(UnitBase owner)
    {
        if (_owner != null)
            Dispose();

        _owner = owner;
        _slider.maxValue = owner.StatusController.MaxHealth.Value;
        _slider.value = owner.StatusController.MaxHealth.Value;
        _shieldSlider.UpdateShield((int)_slider.value, (int)_slider.maxValue, owner.StatusController.Shield.Value);        

        Subcribe();

        _cts = new CancellationTokenSource();
        ChaseOwner(_cts.Token).Forget();
    }

    private async UniTask ChaseOwner(CancellationToken token)
    {
        try
        {
            while (_owner != null && !token.IsCancellationRequested)
            {
                transform.position = _owner.GetBarPosition();
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

    private void Subcribe()
    {
        _owner.StatusController.CurHp.AddEvent(UpdateValue);
        _owner.StatusController.Shield.AddEvent(UpdateShield);
        _owner.StatusController.OnDied += BarDestroy;
    }

    private void Dispose()
    {
        if (_owner != null)
        {
            _owner.StatusController.CurHp.RemoveEvent(UpdateValue);
            _owner.StatusController.Shield.RemoveEvent(UpdateShield);
            _owner.StatusController.OnDied -= BarDestroy;
        }

        _owner = null;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private void UpdateValue(int value)
    {
        _slider.value = value;
    }

    private void UpdateShield(int value)
    {
        _shieldSlider.UpdateShield(_owner.StatusController.CurHp.Value, _owner.StatusController.MaxHealth.Value, value);
    }

    public void BarDestroy()
    {
        Dispose();
        Manager.Resources.Destroy(gameObject);
    }
}
