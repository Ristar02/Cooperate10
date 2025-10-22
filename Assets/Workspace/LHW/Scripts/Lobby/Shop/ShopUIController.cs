using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUIController : MonoBehaviour
{
    // 상점 UI 로 추가되어야 할 것으로 예상되는 내용

    // 1. 상점 아이템 리스트를 받아 아이템 목록 및 수량을 변경
    // 2. 상점에서 아이템을 선택했을 때 해당 아이템을 살 것인지 말 것인지에 대한 팝업 설정 - PopUpManager로 설정 가능
    // 3. 일일 상점 초기화, 광고 보고 상점 아이템 구매 요소 등

    Coroutine _dailyCooltimeTimer;

    [SerializeField] TMP_Text _dailyResetText;
    [SerializeField] private Button _testButton;

    [Header("SlotData")]
    [SerializeField] private ShopItemSO _itemDB;
    [SerializeField] private ShopSlot _slotPrefab;
    [SerializeField] private Transform _dailyList;
    [SerializeField] private Transform _goldList;
    [SerializeField] private Transform _diamondList;

    [Header("Reroll")]
    [SerializeField] private Button _rerollButton;
    [SerializeField] private TMP_Text _rerollCountText;
    [SerializeField] private TMP_Text _rerollPriceText;
    [SerializeField] private Image _rerollPriceImage;
    [SerializeField] private Sprite _goldSprite;
    [SerializeField] private Sprite _diamondSprite;

    [Header("Ad Reroll")]
    [SerializeField] private Button _adRerollButton;
    [SerializeField] private TMP_Text _adRerollCountText;
    [SerializeField] GoogleAdMob _googleAdMob;

    private ShopSlotFactory _shopSlotFactory = new ShopSlotFactory();
    private DailyShopManager _dailyManager;

    private bool _initialized = false; 


    private void Awake()
    {
        _dailyManager = GetComponent<DailyShopManager>();
    }

    private async void Start()
    {
        UserShopData userData = await Manager.DB.shopDB.LoadUserShopDataAsync();

        if (userData != null)
        {
            // DB 데이터 -> 인게임 데이터로 변환/적용
            Manager.DB.shopDB.ApplyUserShopData(
                userData,
                out List<ShopSlotData> dailyList,
                out List<ShopSlotData> goldList,
                out List<ShopSlotData> diamondList,
                out int rerollCount,
                out int adRerollCount,
                _itemDB
            );

            _dailyManager.SetSlotData(dailyList, rerollCount, adRerollCount);
            _dailyManager.SetGoldSlots(goldList);
            _dailyManager.SetDiamondSlots(diamondList);
        }

        await InitShopUI();

        _rerollButton.onClick.AddListener(OnClickRefresh);
        _testButton.onClick.AddListener(RerollShopData);
        _testButton.onClick.AddListener(OnClickRefresh);
        _adRerollButton.onClick.AddListener(OnClickAdReroll);

        // 새로고침 횟수 UI 초기화
        UpdateRerollButtonUI();
        UpdateAdRerollButtonUI();

        _initialized = true; 
    }

    private void OnEnable()
    {
        StartDailyTimer();
    }

    private void OnDisable()
    {
        StopDailyTimer();
    }

    private void StartDailyTimer()
    {
        if (_dailyCooltimeTimer != null)
        {
            StopCoroutine(_dailyCooltimeTimer);
        }
        _dailyCooltimeTimer = StartCoroutine(DailyCooltimeCoroutine());
    }

    private void StopDailyTimer()
    {
        if (_dailyCooltimeTimer != null)
        {
            StopCoroutine(_dailyCooltimeTimer);
            _dailyCooltimeTimer = null;
        }
    }

    private IEnumerator DailyCooltimeCoroutine()
    {
        while (true)
        {
            if (TimeManager.Instance != null)
            {
                bool isResetTime = TimeManager.Instance.LoadDailyShopResetTime(out DateTime nextDate);

                if (_initialized && isResetTime)
                {
                    Debug.Log("Daily Reset 실행");
                    RerollShopData();
                    OnClickRefresh();
                }

                DateTime now = DateTime.Now;
                TimeSpan cooltime = nextDate - now;
                _dailyResetText.text = $"다음 초기화 : {cooltime.Hours}시간 {cooltime.Minutes}분";
            }

            yield return new WaitForSeconds(1);
        }
    }

    #region Init Shop

    /// <summary>
    /// 상점 UI 초기화 (Daily / Gold / Diamond 슬롯 세팅)
    /// </summary>
    private async Task InitShopUI()
    {
        // Daily
        await InitDailyShop();

        // Gold
        InitGoldShop();

        // Diamond
        InitDiamondShop();
    }

    private async Task InitDailyShop()
    {
        // 기존 슬롯 제거
        foreach (Transform child in _dailyList)
        {
            Destroy(child.gameObject);
        }

        // Daily 슬롯 로드
        List<ShopSlotData> dailySlots = _dailyManager.GetDailySlots();

        // DB에 데이터 o -> 생성
        if (dailySlots != null && dailySlots.Count > 0)
        {
            foreach (var slot in dailySlots)
            {
                Instantiate(_slotPrefab, _dailyList).SetSlot(slot);
            }
            return; 
        }

        // DB에 데이터 x -> 생성
        dailySlots = await _dailyManager.GetDailySlotsAsync();
        _dailyManager.SetSlotData(dailySlots, _dailyManager.RerollCount, _dailyManager.AdRerollCount);

        foreach (var slot in dailySlots)
        {
            Instantiate(_slotPrefab, _dailyList).SetSlot(slot);
        }
    }

    private void InitGoldShop()
    {
        foreach (Transform child in _goldList) Destroy(child.gameObject);

        List<ShopSlotData> goldSlots = _dailyManager.GetGoldSlots();

        if (goldSlots != null && goldSlots.Count > 0)
        {
            foreach (var slot in goldSlots)
            {
                Instantiate(_slotPrefab, _goldList).SetSlot(slot);
            }
            return;
        }

        // DB 값이 없을 경우 → 새로 생성
        goldSlots = new List<ShopSlotData>();
        foreach (var meta in _itemDB.Items)
        {
            if (meta != null && meta.Type == ShopType.Gold)
            {
                ShopSlotData slot = _shopSlotFactory.FromGold(meta.ItemId, _itemDB);
                goldSlots.Add(slot);
                Instantiate(_slotPrefab, _goldList).SetSlot(slot);
            }
        }
        _dailyManager.SetGoldSlots(goldSlots);
    }

    private void InitDiamondShop()
    {
        foreach (Transform child in _diamondList)
            Destroy(child.gameObject);

        List<ShopSlotData> diamondSlots = _dailyManager.GetDiamondSlots();

        if (diamondSlots != null && diamondSlots.Count > 0)
        {
            foreach (var slot in diamondSlots)
            {
                Instantiate(_slotPrefab, _diamondList).SetSlot(slot);
            }
            return;
        }

        // DB 값이 없을 경우 → 새로 생성
        diamondSlots = new List<ShopSlotData>();
        foreach (var meta in _itemDB.Items)
        {
            if (meta != null && meta.Type == ShopType.Diamond)
            {
                ShopSlotData slot = _shopSlotFactory.FromDiamond(meta.ItemId, _itemDB);
                diamondSlots.Add(slot);
                Instantiate(_slotPrefab, _diamondList).SetSlot(slot);
            }
        }

        _dailyManager.SetDiamondSlots(diamondSlots);
    }

    #endregion


    #region currency

    /// <summary>
    /// 일일 상점을 새로고침하고 슬롯 UI를 갱신하는 메서드
    /// 버튼 상태를 업데이트
    /// </summary>
    private async void OnClickRefresh()
    {
        if (!_dailyManager.CanReroll())
        {
            _rerollButton.interactable = false;
            return;
        }

        List<ShopSlotData> newSlots = await _dailyManager.RerollDailySlotAsync();

        if (newSlots != null)
        {
            foreach (Transform child in _dailyList) Destroy(child.gameObject);
            foreach (var slot in newSlots) Instantiate(_slotPrefab, _dailyList).SetSlot(slot);
        }

        UpdateRerollButtonUI();

        if (!_dailyManager.CanReroll()) _rerollButton.interactable = false;
    }

    /// <summary>
    /// 새로고침 버튼 UI 갱신하는 메서드
    /// </summary>
    private void UpdateRerollButtonUI()
    {
        int stageCount = _dailyManager.GetStageCount();
        int stageMax = _dailyManager.GetStageMax();
        (int cost, string currency) = _dailyManager.GetRefreshCost();

        _rerollCountText.text = $"{stageCount}/{stageMax}";
        _rerollButton.interactable = _dailyManager.CanReroll();

        // 가격 텍스트/아이콘 갱신
        if (currency == "Free")
        {
            _rerollPriceText.text = "무료";
            _rerollPriceImage.enabled = false;
            RectTransform rect = _rerollPriceText.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(9f, rect.anchoredPosition.y);
        }
        else
        {
            _rerollPriceText.text = cost.ToString();
            _rerollPriceImage.sprite = (currency == "Gold") ? _goldSprite : _diamondSprite;
            _rerollPriceImage.enabled = true;
            RectTransform rect = _rerollPriceText.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(58f, rect.anchoredPosition.y);
        }
    }

    #endregion


    #region AD

    /// <summary>
    /// 광고시청 새로고침 버튼 클릭 시 실행되는 메서드
    /// </summary>
    private void OnClickAdReroll()
    {
        if (!_dailyManager.CanAdReroll())
        {
            _adRerollButton.interactable = false;
            return;
        }

        _googleAdMob.ShowAd(OnAdWatched);
    }

    private async void OnAdWatched()
    {
        // 광고 끝난 후 count 증가 / 새로고침
        await _dailyManager.RerollDailySlotByAdAsync();

        // DailyRandomItems 기준으로 UI 갱신
        List<ShopSlotData> currentSlots = _dailyManager.GetDailySlots();

        foreach (Transform child in _dailyList)
        {
            Destroy(child.gameObject);
        }

        foreach (var slot in currentSlots)
        {
            Instantiate(_slotPrefab, _dailyList).SetSlot(slot);
        }

        UpdateAdRerollButtonUI();
    }

    /// <summary>
    /// 광고시청 새로고침 버튼 UI 갱신하는 메서드
    /// </summary>
    private void UpdateAdRerollButtonUI()
    {
        int current = _dailyManager.GetAdRefreshCount();
        int max = 2;

        _adRerollCountText.text = $"{current}/{max}";
        _adRerollButton.interactable = _dailyManager.CanAdReroll();
    }

    #endregion

    /// <summary>
    /// 일일상점을 초기화하는 메서드
    /// </summary>
    private async void RerollDailyShop()
    {
        await InitDailyShop();
        _dailyManager.ResetRerollCount();
        _dailyManager.ResetAdRerollCount();

        UpdateRerollButtonUI();
        UpdateAdRerollButtonUI();
    }

    /// <summary>
    /// 상점 전체를 초기화하는 메서드
    /// 타이머 초기화 시 호출
    /// </summary>
    private void RerollShopData()
    {
        // Gold, Diamond 슬롯 로드
        InitGoldShop();
        InitDiamondShop();

        // Gold 슬롯 구매 상태 초기화
        List<ShopSlotData> goldSlots = _dailyManager.GetGoldSlots();
        foreach (var slot in goldSlots)
        {
            slot.IsPurchased = false;
        }

        // Diamond 슬롯 구매 상태 초기화
        List<ShopSlotData> diamondSlots = _dailyManager.GetDiamondSlots();
        foreach (var slot in diamondSlots)
        {
            slot.IsPurchased = false;
        }

        _dailyManager.SaveShopData();

        // 새로고침 버튼 횟수 초기화, 일일상점 상품 리스트 초기화
        RerollDailyShop();

        // UI 갱신
        InitGoldShop();
        InitDiamondShop();
    }
}