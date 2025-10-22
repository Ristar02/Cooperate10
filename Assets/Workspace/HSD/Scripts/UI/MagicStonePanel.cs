using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagicStonePanel : MonoBehaviour
{
    [SerializeField] MagicStoneSlot[] _magicStoneSlots;
    [SerializeField] Transform _dropArea;
    [SerializeField] List<MagicStoneSlot> _activeSlots = new List<MagicStoneSlot>();

#if UNITY_EDITOR
    [SerializeField] MagicStoneData _testMagicStoneData;

    [Button("마법석 추가")]
    private void AddTestMagicStone()
    {
        if (_testMagicStoneData == null) return;
        TrySetMagicStone(_testMagicStoneData);
    }

    [Button("제일 앞 마법석 삭제")]
    private void RemoveFirstMagicStone()
    {
        if (_magicStoneSlots.Length == 0) return;

        for (int i = 0; i < _magicStoneSlots.Length; i++)
        {
            if (_magicStoneSlots[i].MagicStoneData != null)
            {
                _magicStoneSlots[i].ClearMagicStone();
                RefreshSlotOrder();
                return;
            }
        }
    }
#endif

    private void Start()
    {
        foreach (var slot in _magicStoneSlots)
        {
            slot.Init(_dropArea);
            slot.OnCleared += HandleSlotCleared;
        }
    }

    public bool TrySetMagicStone(MagicStoneData magicStoneData)
    {
        foreach (var slot in _magicStoneSlots)
        {
            if (slot.MagicStoneData == null)
            {
                slot.SetMagicStone(magicStoneData);
                _activeSlots.Add(slot);
                RefreshSlotOrder();
                return true;
            }
        }

        return false;
    }

    public void SetMagicStone(int idx, MagicStoneData magicStoneData)
    {        
        _magicStoneSlots[idx].SetMagicStone(magicStoneData);
    }

    public void ClearAllMagicStones()
    {
        foreach (var slot in _magicStoneSlots)
            slot.ClearMagicStone();

        _activeSlots.Clear();

        RefreshSlotOrder();
    }

    private void RefreshSlotOrder()
    {
        for (int i = 0; i < _activeSlots.Count; i++)
        {
            _activeSlots[i].transform.SetSiblingIndex(i);
        }
    }

    private void HandleSlotCleared(MagicStoneSlot slot)
    {
        _activeSlots.Remove(slot);
        RefreshSlotOrder();
    }
}
