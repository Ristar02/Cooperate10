using System;
using System.Collections.Generic;

[Serializable]
public class UserShopData
{
    public int RerollCount;       
    public int AdRerollCount;    

    public List<ShopSlotDTO> DailyList;   // 일일 상점 슬롯
    public List<ShopSlotDTO> GoldList;    // 골드 상점 슬롯
    public List<ShopSlotDTO> DiamondList; // 다이아 상점 슬롯

    public UserShopData(int reroll, int adReroll, List<ShopSlotDTO> daily, List<ShopSlotDTO> gold, List<ShopSlotDTO> diamond)
    {
        RerollCount = reroll;
        AdRerollCount = adReroll;
        DailyList = daily;
        GoldList = gold;
        DiamondList = diamond;
    }
}