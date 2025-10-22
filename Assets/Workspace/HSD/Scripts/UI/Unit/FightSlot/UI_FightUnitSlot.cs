using UnityEngine;
using UnityEngine.UI;

public class UI_FightUnitSlot : MonoBehaviour, IFightUnitSlotView
{
    [SerializeField] Image _icon;
    [SerializeField] Slider _hpSlider;
    [SerializeField] Slider _mpSlider;

    private FightUnitSlotPresenter _presenter;

    public void Init(UnitStatusController status)
    {   
        if(status == null)
        {
            _icon.sprite = null;
            _icon.color = Color.clear;
            _hpSlider.value = 0;
            _mpSlider.value = 0;
            return;
        }

        if (_presenter != null)
            _presenter.Dispose();

        _presenter = new FightUnitSlotPresenter(this, status);
    }

    public void SetIcon(Sprite iconSprite)
    {
        _icon.sprite = iconSprite;
        _icon.color = Color.white;
    }

    public void SetHp(float value, float max)
    {
        _hpSlider.maxValue = max;
        _hpSlider.value = value;
    }

    public void SetMana(float value, float max)
    {
        _mpSlider.maxValue = max;
        _mpSlider.value = value;
    }
}