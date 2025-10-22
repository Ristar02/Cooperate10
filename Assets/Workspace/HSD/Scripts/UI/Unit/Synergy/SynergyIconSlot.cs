using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SynergyIconSlot : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] Image _icon;
    [SerializeField] Image _upgradeColorImage;
    private SynergyData _synergyData;
    private SynergyToolTip _synergyToolTip;
    public int ActiveCount;
    public int UpgradeCount => _synergyData.CurrentUpgradeIdx;

    public void Init(SynergyData data, SynergyToolTip synergyToolTip)
    {
        _synergyData = data;
        _icon.sprite = data.Icon;
        _synergyToolTip = synergyToolTip;

        UpdateIcon(0);
    }

    public void UpdateIcon(int activeCount)
    {
        ActiveCount = activeCount;
        _upgradeColorImage.color = _synergyData.GetSynergyColor();        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_synergyData == null) return;

        _synergyToolTip.Show(eventData, _synergyData);
    }
}
