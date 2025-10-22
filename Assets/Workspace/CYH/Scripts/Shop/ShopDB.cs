using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Database;

public class ShopDB
{
    private DatabaseReference UserShopRef()
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        return FirebaseManager.DataReference
            .Child("UserData").Child(uid).Child("ShopData");
    }

    // 변환 메서드
    #region Convert

    /// <summary>
    /// ShopSlotData 리스트 -> ShopSlotDTO 리스트로 변환
    /// (Firebase 업로드용)
    /// </summary>
    private List<ShopSlotDTO> ToDTOList(List<ShopSlotData> slots)
    {
        return slots.Select(slot => new ShopSlotDTO(slot)).ToList();
    }

    /// <summary>
    /// ShopSlotDTO 리스트 -> ShopSlotData 리스트로 변환
    /// (Firebase 다운로드 후 인게임 적용용)
    /// </summary>
    private List<ShopSlotData> ToDataList(List<ShopSlotDTO> dtoList, ShopItemSO itemDB)
    {
        ShopSlotFactory factory = new ShopSlotFactory();
        List<ShopSlotData> result = new List<ShopSlotData>();

        foreach (var dto in dtoList)
        {
            ShopSlotData data;

            if (dto.Type == ShopType.Gold)
                data = factory.FromGold(dto.ItemId, itemDB);
            else if (dto.Type == ShopType.Diamond)
                data = factory.FromDiamond(dto.ItemId, itemDB);
            else
            {
                // Daily -> 기존 dto 그대로 변환
                data = new ShopSlotData
                {
                    Type = dto.Type,
                    ItemId = dto.ItemId,
                    ItemPrice = dto.ItemPrice,
                    ItemName = dto.ItemName,
                    Count = dto.Count,
                    UsageCount = dto.UsageCount,
                    IsGold = dto.IsGold,
                    IsDiamond = dto.IsDiamond,
                    IsPurchased = dto.IsPurchased,
                    IsFree = dto.IsFree
                };
            }

            data.IsPurchased = dto.IsPurchased;

            result.Add(data);
        }

        return result;
    }

    #endregion


    // Save
    #region Save

    public async void SaveUserShopDataAsync(
        int rerollCount, int adRerollCount,
        List<ShopSlotData> dailyList,
        List<ShopSlotData> goldList,
        List<ShopSlotData> diamondList)
    {
        UserShopData shopData = new UserShopData(
            rerollCount,
            adRerollCount,
            ToDTOList(dailyList),
            ToDTOList(goldList),
            ToDTOList(diamondList)
        );

        string json = JsonUtility.ToJson(shopData);
        await UserShopRef().SetRawJsonValueAsync(json);

        Debug.Log("SaveUserShopDataAsync 완료");
    }

    #endregion


    // Load
    #region Load

    public async Task<UserShopData> LoadUserShopDataAsync()
    {
        DataSnapshot snapshot = await UserShopRef().GetValueAsync();

        if (!snapshot.Exists)
        {
            Debug.LogWarning("ShopData x / default 생성");

            UserShopData defaultData = new UserShopData(0, 0,
                new List<ShopSlotDTO>(),
                new List<ShopSlotDTO>(),
                new List<ShopSlotDTO>());

            string json = JsonUtility.ToJson(defaultData);
            await UserShopRef().SetRawJsonValueAsync(json);

            return defaultData;
        }

        string jsonData = snapshot.GetRawJsonValue();
        UserShopData data = JsonUtility.FromJson<UserShopData>(jsonData);
        return data;
    }

    /// <summary>
    /// UserShopData -> 실제 게임 데이터 적용용 변환
    /// </summary>
    public void ApplyUserShopData(UserShopData userData,
        out List<ShopSlotData> dailyList,
        out List<ShopSlotData> goldList,
        out List<ShopSlotData> diamondList,
        out int rerollCount,
        out int adRerollCount,
        ShopItemSO itemDB
    )
    {
        dailyList = ToDataList(userData.DailyList, itemDB);
        goldList = ToDataList(userData.GoldList, itemDB);
        diamondList = ToDataList(userData.DiamondList, itemDB);

        rerollCount = userData.RerollCount;
        adRerollCount = userData.AdRerollCount;
    }

    #endregion
}