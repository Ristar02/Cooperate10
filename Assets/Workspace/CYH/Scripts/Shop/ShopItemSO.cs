using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/ItemDB")]
public class ShopItemSO : ScriptableObject
{
    public ShopSlotMeta[] Items;

    public ShopSlotMeta GetMeta(string itemId)
    {
        foreach (ShopSlotMeta item in Items)
        {
            if (item.ItemId == itemId)
                return item;
        }
        return null;
    }
}
