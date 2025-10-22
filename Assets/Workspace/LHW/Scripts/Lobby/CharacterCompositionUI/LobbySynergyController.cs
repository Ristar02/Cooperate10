using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LobbySynergyController : MonoBehaviour
{
    [SerializeField] GameObject _synergySlotPrefab;
    [SerializeField] Transform _content;

    private Dictionary<string, LobbySynergySlot> _synergySlots = new(50);

    private TeamOrganizeManager _manager;

    private void Awake()
    {
        _manager = GetComponentInParent<TeamOrganizeManager>();
    }

    private void OnEnable()
    {
        InitializeSynergySlots();
        _manager.OnCharacterDataChanged += UpdateSynergySlots;
    }

    private void OnDisable()
    {
        _manager.OnCharacterDataChanged -= UpdateSynergySlots;
    }

    private void InitializeSynergySlots()
    {
        if (Manager.Data.SynergyDB == null || _synergySlots.Count != 0) return;

        foreach (var data in Manager.Data.SynergyDB._synergyDataDic.Values)
        {
            if (data is UnitSynergyData unitSynergy)
            {
                LobbySynergySlot slot = Instantiate(_synergySlotPrefab, _content).GetComponent<LobbySynergySlot>();
                slot.Init(data, 0);
                _synergySlots.Add(unitSynergy.Synergy.ToString(), slot);
                Debug.Log(unitSynergy.Synergy.ToString());
            }
        }
    }    

    private void UpdateSynergySlots()
    {
        if (Manager.Data.SynergyDB._synergyDataDic == null || _synergySlots == null) return;

        Dictionary<Synergy, int> synergyCount = _manager.GetSynergyCount();

        foreach(var data in synergyCount)
        {
            Synergy synergy = data.Key;
            int count = data.Value;

            _synergySlots[synergy.ToString()].UpdateUI(count);
        }

        SetHiararchy();
    }

    private void SetHiararchy()
    {
        List<LobbySynergySlot> slots = new List<LobbySynergySlot>(_synergySlots.Values);

        slots.RemoveAll(s => s.ActiveCount <= 0);

        slots.Sort((a, b) =>
        {
            int result = b.ActiveCount.CompareTo(a.ActiveCount);
            if (result == 0)
                result = b.UpgradeCount.CompareTo(a.UpgradeCount);

            return result;
        });

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].transform.SetSiblingIndex(i);
        }
    }
}