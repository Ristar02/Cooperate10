using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class DamageMeterController : MonoBehaviour
{
    [SerializeField] Transform _content;
    [SerializeField] GridLayoutGroup _gridLayoutGroup;
    [SerializeField] DamageMeterSlot[] _damageMeterSlots;
    private List<DamageMeterSlot> _activeSlots = new List<DamageMeterSlot>();
    private CancellationTokenSource _cts;
    private int _unitCount;

    private void Start()
    {
        _gridLayoutGroup.SetupGridLayoutGroup(_content, 1, 5, 10);
        DamageMeterSlot.OnDamaged += SortingDamageMeter;
    }

    private void OnDestroy()
    {
        DamageMeterSlot.OnDamaged -= SortingDamageMeter;
    }

    public void Init(UnitBase[] units)
    {
        int count = 0;
        for (int i = 0; i < units.Length; i++)
        {
            if (units[i] != null)
                count++;
        }

        _unitCount = count;

        for (int i = 0; i < _damageMeterSlots.Length; i++)
        {
            if (units[i] != null && i < _unitCount)
            {
                UnitStatusController status = units[i].StatusController;
                _damageMeterSlots[i].Init(status);
                _damageMeterSlots[i].gameObject.SetActive(false);
            }
            else
            {
                _damageMeterSlots[i].gameObject.SetActive(false);
            }
        }

        SortingDamageMeter();
    }

    private void SortingDamageMeter()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        _activeSlots.Clear();

        foreach (var slot in _damageMeterSlots)
            _activeSlots.Add(slot);

        _activeSlots.Sort((a, b) => b.TotalDamage.CompareTo(a.TotalDamage));

        foreach (var slot in _damageMeterSlots)
            slot.gameObject.SetActive(false);

        // 상위 5개만 활성화
        int count = Mathf.Min(5, _unitCount);

        for (int i = 0; i < count; i++)
        {
            var slot = _activeSlots[i];
            slot.gameObject.SetActive(true);
            slot.transform.SetSiblingIndex(i);
            slot.SetNumber(i + 1);
        }

        RefreshAllSlots();
    }

    private void RefreshAllSlots()
    {
        if (_activeSlots.Count == 0) return;

        int maxValue = _activeSlots[0].TotalDamage;

        foreach (var slot in _damageMeterSlots)
        {
            slot.RefreshValue(maxValue);
        }
    }
}
