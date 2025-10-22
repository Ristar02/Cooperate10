using UnityEngine;
using UnityEngine.UI;

// 사용하지 않는 코드입니다.
public class LobbyClassSynergySlot : MonoBehaviour
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