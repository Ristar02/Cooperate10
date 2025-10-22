using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class DailyShopManager : MonoBehaviour
{
    private string _csvUrl = "https://docs.google.com/spreadsheets/d/1ICgXSMDqSO7-SW-9lKCmgLx3Kq-eA2T2DCf11_ySXV4/export?format=csv&gid=891945452";

    private readonly int[] _goldCosts = { 500, 1000 };
    private readonly int[] _diamondCosts = { 50, 100 };

    // 새로고침 버튼
    private int _rerollCount = 0;
    private const int MaxRerollCount = 5;
    public void ResetRerollCount() => _rerollCount = 0;
    public bool CanReroll() => _rerollCount < MaxRerollCount;
    public int RerollCount
    {
        get => _rerollCount;
        set => _rerollCount = value;
    }

    // 광고 새로고침 버튼
    private int _adRerollCount = 0;
    private const int MaxAdRerollCount = 2;
    public void ResetAdRerollCount() => _adRerollCount = 0;
    public int GetAdRefreshCount() => _adRerollCount;
    public bool CanAdReroll() => _adRerollCount < MaxAdRerollCount;
    public int AdRerollCount
    {
        get => _adRerollCount;
        set => _adRerollCount = value;
    }

    // 데이터 연동용 리스트
    private List<ShopSlotData> DailyRandomItems = new();   // 일일 상점 슬롯
    private List<ShopSlotData> _goldSlots = new();        // 골드 상점 슬롯
    private List<ShopSlotData> _diamondSlots = new();     // 다이아 상점 슬롯

    public List<ShopSlotData> GetDailySlots() => DailyRandomItems;
    public List<ShopSlotData> GetGoldSlots() => _goldSlots;
    public List<ShopSlotData> GetDiamondSlots() => _diamondSlots;

    // Firebase 로드 후 적용
    public void SetSlotData(List<ShopSlotData> dailyList, int reroll, int adReroll)
    {
        DailyRandomItems = dailyList ?? new List<ShopSlotData>();
        _rerollCount = reroll;
        _adRerollCount = adReroll;
    }

    // Gold/Diamond 슬롯 세팅용
    public void SetGoldSlots(List<ShopSlotData> goldSlots)
    {
        _goldSlots = goldSlots;
    }

    public void SetDiamondSlots(List<ShopSlotData> diamondSlots)
    {
        _diamondSlots = diamondSlots;
    }

    public void SaveShopData()
    {
        Manager.DB.shopDB.SaveUserShopDataAsync(
            _rerollCount,
            _adRerollCount,
            GetDailySlots(),
            GetGoldSlots(),
            GetDiamondSlots()
        );
    }

    /// <summary>
    /// 슬롯 타입에 따라 해당 리스트에서 ItemId가 같은 슬롯을 찾아 갱신하는 메서드
    /// 갱신 후 DB 저장
    /// </summary>
    /// <param name="updatedSlot">갱신할 대상 슬롯 데이터</param>
    public void UpdateSlotData(ShopSlotData updatedSlot)
    {
        List<ShopSlotData> targetList = null;

        switch (updatedSlot.Type)
        {
            case ShopType.Daily:
                targetList = DailyRandomItems;
                break;
            case ShopType.Gold:
                targetList = _goldSlots;
                break;
            case ShopType.Diamond:
                targetList = _diamondSlots;
                break;
        }

        if (targetList == null) return;

        for (int i = 0; i < targetList.Count; i++)
        {
            if (targetList[i].ItemId == updatedSlot.ItemId)
            {
                targetList[i] = updatedSlot;
                break;
            }
        }

        // 리스트 갱신 후 DB 저장
        SaveShopData();
    }

    /// <summary>
    ///  일일 상점 슬롯을 새로고침하는 메서드
    ///  0회 : 무료 새로고침
    ///  1~2회 : 골드 차감
    ///  3~4회 : 다이아몬드 차감
    ///  최신 일일 슬롯 데이터를 반환
    /// </summary>
    public async UniTask<List<ShopSlotData>> RerollDailySlotAsync()
    {
        if (_rerollCount >= MaxRerollCount)
        {
            Debug.Log("새로고침 불가: 한도 도달");
            return null;
        }

        if (_rerollCount == 0)
        {
            Debug.Log("새로고침 무료");
        }
        else if (_rerollCount <= 2)
        {
            if(_rerollCount == 1)
            {
                Debug.Log($"골드 {_goldCosts[0]}개 차감");
                await Manager.DB.TrySubtractGoldAsync(_goldCosts[0]);
            }
            else if(_rerollCount == 2)
            {
                Debug.Log($"골드 {_goldCosts[1]}개 차감");
                await Manager.DB.TrySubtractGoldAsync(_goldCosts[1]);
            }
        }
        else if (_rerollCount <= 4)
        {
            if (_rerollCount == 3)
            {
                Debug.Log($"다이아 {_diamondCosts[0]}개 차감");
                await Manager.DB.TrySubtractDiamondAsync(_diamondCosts[0]);
            }
            else if (_rerollCount == 4)
            {
                Debug.Log($"다이아 {_diamondCosts[1]}개 차감");
                await Manager.DB.TrySubtractDiamondAsync(_diamondCosts[1]);
            }
        }

        _rerollCount++;

        // 새 슬롯 뽑은 후 메모리 갱신
        List<ShopSlotData> newSlots = await GetDailySlotsAsync();
        DailyRandomItems = newSlots;

        // reroll 후 저장
        SaveShopData();

        // 현재 DailyRandomItem 리스트 리턴
        return DailyRandomItems;
    }

    /// <summary>
    /// 광고시청으로 일일 상점 새로고침하는 메서드
    /// </summary>
    public async UniTask<List<ShopSlotData>> RerollDailySlotByAdAsync()
    {
        if (_adRerollCount >= MaxAdRerollCount)
        {
            Debug.Log("광고 새로고침 불가: 한도 도달");
            return null;
        }

        _adRerollCount++;
        Debug.Log("새로고침 광고");

        // 새 슬롯 뽑은 후 메모리 갱신
        List<ShopSlotData> newSlots = await GetDailySlotsAsync();
        DailyRandomItems = newSlots;

        // 광고 reroll 후 저장
        SaveShopData();

        // 항상 메모리에 저장된 것만 반환
        return DailyRandomItems;
    }

    /// <summary>
    /// 현재 단계에서 소비해야 할 금액, 화폐종류를 반환하는 메서드
    /// </summary>
    public (int cost, string currency) GetRefreshCost()
    {
        // 무료
        if (_rerollCount == 0) return (0, "Free");

        // 골드 
        if (_rerollCount == 1) return (_goldCosts[0], "Gold");
        if (_rerollCount == 2) return (_goldCosts[1], "Gold");

        // 다이아
        if (_rerollCount == 3) return (_diamondCosts[0], "Diamond");
        if (_rerollCount == 4) return (_diamondCosts[1], "Diamond");

        return (_diamondCosts[1], "Diamond");
    }

    /// <summary>
    /// UI용 현재 단계별 카운트를 반환하는 메서드
    /// </summary>
    public int GetStageCount()
    {
        if (_rerollCount == 0) return 0;
        if (_rerollCount == 1) return 0;

        if (_rerollCount == 2) return 1;
        if (_rerollCount == 3) return 0;

        if (_rerollCount == 4) return 1;
        if (_rerollCount >= 5) return 2;

        return 0;
    }

    /// <summary>
    /// UI용 현재 단계별 최대값을 반환하는 메서드
    /// </summary>
    public int GetStageMax()
    {
        if (_rerollCount == 0) return 1;   // 무료
        if (_rerollCount <= 2) return 2;   // 골드
        if (_rerollCount <= 4) return 2;   // 다이아
        return 2;
    }

    /// <summary>
    /// CSV에서 아이템 불러와 6개 랜덤 뽑는 메서드
    /// </summary>
    public async UniTask<List<ShopSlotData>> GetDailySlotsAsync()
    {
        DailyShopCSV loader = new DailyShopCSV(_csvUrl);
        List<ShopCSVData> items = await loader.LoadAsync(1, 46);

        if (items == null || items.Count == 0)
        {
            Debug.LogError("ShopCSVData 로드 실패");
            return new List<ShopSlotData>();
        }

        List<ShopSlotData> result = new List<ShopSlotData>();
        List<ShopCSVData> randomItems = PickRandomItems(items, 6);

        foreach (var item in randomItems)
        {
            result.Add(ConvertToSlot(item));
        }

        return result;
    }

    /// <summary>
    /// CSV 데이터를 ShopSlotData로 변환하는 메서드
    /// 개수, currency 종류 랜덤 
    /// </summary>
    private ShopSlotData ConvertToSlot(ShopCSVData data)
    {
        int totalPrice;
        int count = Random.Range(1, 9);
        bool useGold = Random.value < 0.5f;
        string priceText;
        string countText = $"{count}";

        if (useGold)
        {
            totalPrice = data.GoldPrice * count;
            priceText = $"{totalPrice}";

            return new ShopSlotData
            {
                Type = ShopType.Daily,
                ItemId = data.ItemID.ToString(),
                Count = countText,
                ItemName = data.ItemName,
                ItemPrice = priceText,
                IsGold = true
            };
        }
        else
        {
            totalPrice = data.DiamondPrice * count;
            priceText = $"{totalPrice}";

            return new ShopSlotData
            {
                Type = ShopType.Daily,
                ItemId = data.ItemID.ToString(),
                Count = countText,
                ItemName = data.ItemName,
                ItemPrice = priceText,
                IsDiamond = true
            };
        }
    }

    /// <summary>
    /// 확률에 따라 랜덤하게 아이템을 선택해서 리스트로 반환하는 메서드
    /// </summary>
    /// <param name="items">대상 아이템 리스트</param>
    /// <param name="count">뽑을 아이템 개수</param>
    /// <returns>랜덤하게 선택된 아이템 리스트</returns>
    private List<ShopCSVData> PickRandomItems(List<ShopCSVData> items, int count)
    {
        List<ShopCSVData> result = new List<ShopCSVData>();

        for (int i = 0; i < count; i++)
        {
            float totalProb = 0f;

            foreach (var item in items)
            {
                totalProb += item.ItemProb;
            }

            float rand = Random.Range(0, totalProb);
            float accumulatedProb = 0f;

            foreach (var item in items)
            {
                accumulatedProb += item.ItemProb;
                if (rand <= accumulatedProb)
                {
                    result.Add(item);
                    break;
                }
            }
        }
        return result;
    }
}