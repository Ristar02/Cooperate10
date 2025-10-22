using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageMeterSlot : MonoBehaviour, IDamageMeterView
{
    [SerializeField] Image _icon;
    [SerializeField] Slider _damageSlider;
    [SerializeField] TMP_Text _numberText;
    [SerializeField] TMP_Text _damageText;
    [SerializeField] int _totalDamage;
    public int TotalDamage => _totalDamage;

    private DamageMeterPresenter _presenter;

    public static event Action OnDamaged;

    private void Awake()
    {
        _damageSlider.value = 1;

        // 항상 비율 기반으로 쓸 거니까 maxValue = 1 고정
        _damageSlider.maxValue = 1f;
    }

    public void Init(UnitStatusController status)
    {
        if (_presenter != null)
            _presenter.Dispose();

        _presenter = new DamageMeterPresenter(this, status);
    }

    public void SetDamage(int damage)
    {
        _totalDamage = damage;
        _damageText.LerpText(damage);
        OnDamaged?.Invoke();
    }

    public void SetIcon(Sprite icon)
    {
        _icon.sprite = icon;
    }

    public void SetNumber(int num)
    {
        _numberText.text = num.ToString();
    }

    public void RefreshValue(int maxDamage)
    {
        float ratio = maxDamage > 0 ? (float)_totalDamage / maxDamage : 0f;
        _damageSlider.Lerp(ratio);
    }
}
