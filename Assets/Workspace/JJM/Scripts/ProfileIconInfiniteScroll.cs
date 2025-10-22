using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileIconInfiniteScroll : InfiniteScrollBase<ProfileIconSlot, UnitData>
{
    protected override void SetSlotData(ProfileIconSlot slot, UnitData data, int index)
    {
        slot.SetData(data);
    }

    public void SetDataList(List<UnitData> dataList)
    {
        allData = dataList;
        Reset();
    }
}
