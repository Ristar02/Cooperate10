using UnityEngine;
using UnityEngine.UI;

public class LobbySynergySlot : MonoBehaviour
{
    [SerializeField] private Image _icon;

    private SynergyData _synergyData;
    private int[] _synergyCountArray;
    public int ActiveCount { get; private set; }
    public int UpgradeCount => _synergyData.CurrentUpgradeIdx;

    public void Init(SynergyData data, int activeCount)
    {        
        _synergyData = data;
        _icon.sprite = data.Icon;
        SetSynergyCount();
        UpdateUI(activeCount);
    }

    public void UpdateUI(int activeCount)
    {
        ActiveCount = activeCount;
        if(ActiveCount < _synergyCountArray[0])
        {
            _icon.color = Color.clear;
        }
        else
        {
            _icon.color = Color.white;
        }
    }

    public void SetSynergyCount()
    {
        _synergyCountArray = new int[_synergyData.SynergyLevelData.Length];

        for (int i = 0; i < _synergyData.SynergyLevelData.Length; i++)
        {
            _synergyCountArray[i] = _synergyData.SynergyLevelData[i].SynergyNeedCount;
        }
    }
}