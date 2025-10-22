using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SynergySlotPanel : MonoBehaviour
{    
    [SerializeField] SynergyToolTip _synergyTooltip;
    [SerializeField] SynergyIconSlot[] _synergyIconSlots;
    private Dictionary<int, int> _synergyIconSlotDic = new(10);

    public void Init(SynergyDatabase db)
    {
        CreateSynergtSlots(db);
    }

    private void CreateSynergtSlots(SynergyDatabase db)
    {
        SynergyData[] datas = db._synergyDataDic.Values.ToArray();

        for (int i = 0; i < datas.Length; i++)
        {
            SynergyIconSlot slot = _synergyIconSlots[i];

            slot.Init(datas[i], _synergyTooltip);
            
            _synergyIconSlots[i] = slot;

            if (datas[i] is ClassSynergyData classSynergy)
                _synergyIconSlotDic.Add((int)classSynergy.Synergy, i);
            else if (datas[i] is UnitSynergyData unitSynergy)
                _synergyIconSlotDic.Add((int)unitSynergy.Synergy, i);
        }        
    }

    public void UpdateSynergySlot(int synergy, int activeCount)
    {
        _synergyIconSlots[_synergyIconSlotDic[synergy]].UpdateIcon(activeCount);
        SetHiararchy();
    }


    private void SetHiararchy()
    {
        List<SynergyIconSlot> slots = _synergyIconSlots.ToList();

        slots.RemoveAll(s => s.ActiveCount <= 0);

        slots.Sort((a, b) =>
        {
            int result = b.ActiveCount.CompareTo(a.ActiveCount);
            if(result == 0)
                result = b.UpgradeCount.CompareTo(a.UpgradeCount);

            return result;
        });

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].transform.SetSiblingIndex(i);
        }
    }
}
