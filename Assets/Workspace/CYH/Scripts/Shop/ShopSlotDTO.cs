using System;

[Serializable]
public class ShopSlotDTO
{
    public ShopType Type;
    public string ItemId;
    public string ItemPrice;
    public string ItemName;
    public string Count;
    public int UsageCount;

    public bool IsGold;
    public bool IsDiamond;
    public bool IsPurchased;
    public bool IsFree;

    public ShopSlotDTO(ShopSlotData data)
    {
        Type = data.Type;
        ItemId = data.ItemId;
        ItemPrice = data.ItemPrice;
        ItemName = data.ItemName;
        Count = data.Count;
        UsageCount = data.UsageCount;

        IsGold = data.IsGold;
        IsDiamond = data.IsDiamond;
        IsPurchased = data.IsPurchased;
        IsFree = data.IsFree;
    }
}