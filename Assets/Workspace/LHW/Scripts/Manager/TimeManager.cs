using System;
using UnityEngine;

public enum GachaType
{
    Char,
    Stone,
}

[Serializable]
public class RewardInfo
{
    public long dateTicks; // DateTime을 Ticks로 저장
    public int state;      // 획득 여부 또는 스택 수

    /// <summary>
    /// 리워드 정보를 생성함.
    /// </summary>
    /// <param name="dateTicks">시간을 Long으로 변환</param>
    /// <param name="state">획득여부 혹은 Stack수</param>
    public RewardInfo(long dateTicks, int state)
    {
        this.dateTicks = dateTicks;
        this.state = state;
    }

    /// <summary>
    /// 시간 정보를 로드
    /// </summary>
    /// <returns></returns>
    public DateTime GetDateTime()
    {
        return new DateTime(dateTicks);
    }

    /// <summary>
    /// 시간 정보를 저장
    /// </summary>
    /// <param name="dateTime"></param>
    public void SetDateTime(DateTime dateTime)
    {
        dateTicks = dateTime.Ticks;
    }
}

public class TimeManager : MonoBehaviour
{
    #region Singleton
    public static TimeManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Init();
    }
    #endregion

    [Header("Reference")]
    [SerializeField] private GoogleAdMob _adMob;

    // 캐릭터 가챠
    private RewardInfo _dailyCharFreeGachaRewardInfo;
    public RewardInfo DailyCharFreeGachaRewardInfo => _dailyCharFreeGachaRewardInfo;

    private RewardInfo _dailyCharAdGachaRewardInfo;
    public RewardInfo DailyCharAdGachaRewardInfo => _dailyCharAdGachaRewardInfo;

    // 마법석 가챠
    private RewardInfo _dailyStoneFreeGachaRewardInfo;
    public RewardInfo DailyStoneFreeGachaRewardInfo => _dailyStoneFreeGachaRewardInfo;

    private RewardInfo _dailyStoneAdGachaRewardInfo;
    public RewardInfo DailyStoneAdGachaRewardInfo => _dailyStoneAdGachaRewardInfo;

    // 상점 
    private DateTime _dailyShopResetTime;

    public Action OnDailyGachaInfoChanged;

    private void Init()
    {
        LoadDailyFreeGachaResetTimeInfo();
        LoadAdGachaResetTimeInfo();
    }

    #region Data Load & Save

    #region 일일 초기화(가챠 - 오전 6시 초기화)

    /// <summary>
    /// 일일 가챠 초기화 시간 및 횟수를 캐싱하여 저장하고,
    /// 업데이트가 필요할 시 업데이트를 바로 진행.
    /// </summary>
    private void LoadDailyFreeGachaResetTimeInfo()
    {
        // TODO : DB에 저장된 [일일 무료 가챠] 시간 및 횟수 데이터를 가져와서 캐싱

        // 테스트용: 오늘 아침 6시, 가챠횟수 1회
        DateTime todayReset = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 0);
        _dailyCharFreeGachaRewardInfo = new RewardInfo(todayReset.Ticks, 1);
        _dailyStoneFreeGachaRewardInfo = new RewardInfo(todayReset.Ticks, 1);

        if (_dailyCharFreeGachaRewardInfo.state == 0 && IsDailyResetTime(_dailyCharFreeGachaRewardInfo.GetDateTime()))
        {
            _dailyCharFreeGachaRewardInfo.state = 1;

            // TODO : DB에 [일일 무료 가챠] 시간과 상태를 저장 
        }
        if(_dailyStoneFreeGachaRewardInfo.state == 0 && IsDailyResetTime(_dailyStoneFreeGachaRewardInfo.GetDateTime()))
        {
            _dailyStoneFreeGachaRewardInfo.state = 1;

            // TODO : DB에 [일일 무료 가챠] 시간과 상태를 저장
        }

        OnDailyGachaInfoChanged?.Invoke();
    }

    /// <summary>
    /// 일일 무료 가챠 시간을 업데이트 - 횟수 사용 시
    /// </summary>
    public void UpdateDailyFreeGachaResetTimeInfo(GachaType type)
    {
        DateTime now = DateTime.Now;
        DateTime todayReset = new DateTime(now.Year, now.Month, now.Day, 6, 0, 0);
        DateTime nextResetDate = (now.Hour < 6) ? todayReset : todayReset.AddDays(1);

        switch(type)
        {
            case GachaType.Char:
                _dailyCharFreeGachaRewardInfo.SetDateTime(nextResetDate);
                _dailyCharFreeGachaRewardInfo.state = 0;

                // TODO : DB에 [일일 무료 가챠] 시간과 상태를 저장
                break;
            case GachaType.Stone:
                _dailyStoneFreeGachaRewardInfo.SetDateTime(nextResetDate);
                _dailyStoneFreeGachaRewardInfo.state = 0;

                // TODO : DB에 [일일 무료 가챠] 시간과 상태를 저장
                break;
        }

        OnDailyGachaInfoChanged?.Invoke();
    }

    #endregion

    #region 12시간 초기화(가챠)

    /// <summary>
    /// 광고 가챠 시간 및 횟수를 캐싱하여 저장하고,
    /// 업데이트가 필요할 시 바로 진행.
    /// </summary>
    private void LoadAdGachaResetTimeInfo()
    {
        // TODO : DB에 저장된 [광고 가챠] 시간 및 횟수 데이터를 가져와서 캐싱

        // 테스트용 초기값: 11시간 전, 가챠 횟수 1회
        _dailyCharAdGachaRewardInfo = new RewardInfo(DateTime.Now.AddHours(-11).AddMinutes(-59).Ticks, 1);
        _dailyStoneAdGachaRewardInfo = new RewardInfo(DateTime.Now.AddHours(-11).AddMinutes(-59).Ticks, 1);

        if (_dailyCharAdGachaRewardInfo.state < 2 && IsDailyCharAdGachaResetTime(out int stack))
        {
            _dailyCharAdGachaRewardInfo.state += stack;
            if (_dailyCharAdGachaRewardInfo.state > 2) _dailyCharAdGachaRewardInfo.state = 2;

            // TODO : DB에 [광고 가챠] 시간과 상태를 저장
        }
        if(_dailyStoneAdGachaRewardInfo.state < 2 && IsDailyCharAdGachaResetTime(out int stack2))
        {
            _dailyStoneAdGachaRewardInfo.state += stack2;
            if (_dailyStoneAdGachaRewardInfo.state > 2) _dailyStoneAdGachaRewardInfo.state = 2;

            // TODO : DB에 [광고 가챠] 시간과 상태를 저장
        }

        OnDailyGachaInfoChanged?.Invoke();
    }

    /// <summary>
    /// 광고 가챠 횟수를 업데이트 - 횟수 사용 시
    /// </summary>
    public void UpdateAdGachaResetTimeInfo(GachaType type)
    {
        switch(type)
        {
            case GachaType.Char:
                UpdateCharAdGachaResetTimeInfo();
                break;
            case GachaType.Stone:
                UpdateStoneAdGachaResetTimeInfo();
                break;
        }
        OnDailyGachaInfoChanged?.Invoke();
    }

    private void UpdateCharAdGachaResetTimeInfo()
    {
        if (_dailyCharAdGachaRewardInfo.state > 0)
        {
            // 해당 부분은 RandomGachaSystem으로 옮기는 게 적절해 보임
            // 추후에 옮길 예정
            if (_adMob.IsReady)
            {
                _adMob.LoadedAd.Show();
            }
            _dailyCharAdGachaRewardInfo.state -= 1;

            // TODO : DB에 [광고 가챠] 시간과 상태를 저장

            Debug.Log($"광고 가챠 스택 감소: {_dailyCharAdGachaRewardInfo.state}, 마지막 갱신: {_dailyCharAdGachaRewardInfo.GetDateTime()}");
        }
    }

    private void UpdateStoneAdGachaResetTimeInfo()
    {
        if (_dailyStoneAdGachaRewardInfo.state > 0)
        {
            // 해당 부분은 RandomGachaSystem으로 옮기는 게 적절해 보임
            // 추후에 옮길 예정
            if (_adMob.IsReady)
            {
                _adMob.LoadedAd.Show();
            }
            _dailyStoneAdGachaRewardInfo.state -= 1;

            // TODO : DB에 [광고 가챠] 시간과 상태를 저장

            Debug.Log($"광고 가챠 스택 감소: {_dailyStoneAdGachaRewardInfo.state}, 마지막 갱신: {_dailyCharAdGachaRewardInfo.GetDateTime()}");
        }
    }

    #endregion

    #region 일일 초기화(상점)

    // 상점의 경우 일일 초기화가 진행되는 시점인지 bool 여부만 판정하면 되므로
    // 가챠확률 계산과는 로직을 다르게 설계했습니다.
    // 만약 일일 퀘스트 등의 다른 컨텐츠도 초기화 시간이 같을 경우 이 함수로 전부 처리 가능합니다.

    /// <summary>
    /// 초기화가 되는 시점인지 확인하고 정보를 로드
    /// </summary>
    /// <param name="resetTime"></param>
    /// <returns></returns>
    public bool LoadDailyShopResetTime(out DateTime resetTime)
    {
        // TODO : DB에 저장된 [상점 초기화] 시간 데이터를 가져와서 캐싱
        // _dailyShopResetTime = {상점 초기화 시간}

        bool isResetTime = SaveDailyShopResetTime(_dailyShopResetTime);
        resetTime = _dailyShopResetTime;

        return isResetTime;
    }

    /// <summary>
    /// 초기화가 되는 시점일 때 업데이트를 진행함
    /// </summary>
    /// <param name="lastTime"></param>
    /// <returns></returns>
    private bool SaveDailyShopResetTime(DateTime lastTime)
    {
        if (IsDailyResetTime(lastTime))
        {
            DateTime now = DateTime.Now;
            DateTime todayReset = new DateTime(now.Year, now.Month, now.Day, 6, 0, 0);

            if (now.Hour < 6)
            {
                _dailyShopResetTime = todayReset;
            }
            else
            {
                _dailyShopResetTime = todayReset.AddDays(1);
            }

            // TODO : DB에 [상점 초기화] 시간을 저장
            return true;
        }
        return false;
    }

    #endregion

    #endregion

    #region Obtain 판정

    #region 일일 가챠 가능 여부 판정

    /// <summary>
    /// 일일 가챠가 가능한 상태인지 판별함.
    /// </summary>
    /// <returns></returns>
    public bool CanObtainedFreeGachaReward(GachaType type)
    {
        bool canObtain = false;

        switch (type)
        {
            case GachaType.Char:
                canObtain = CanObtainFreeCharGachaReward();
                break;
            case GachaType.Stone:
                canObtain = CanObtainFreeStoneGachaReward();
                break;                
        }

        return canObtain;
    }

    private bool CanObtainFreeCharGachaReward()
    {
        if (_dailyCharFreeGachaRewardInfo.state == 1) return true;
        if (IsDailyResetTime(_dailyCharFreeGachaRewardInfo.GetDateTime()))
        {
            _dailyCharFreeGachaRewardInfo.state = 1;

            // TODO : DB에 [일일 무료 가챠] 시간과 상태를 저장

            OnDailyGachaInfoChanged?.Invoke();
            return true;
        }
        return false;
    }

    private bool CanObtainFreeStoneGachaReward()
    {
        if (_dailyStoneFreeGachaRewardInfo.state == 1) return true;
        if (IsDailyResetTime(_dailyStoneFreeGachaRewardInfo.GetDateTime()))
        {
            _dailyStoneFreeGachaRewardInfo.state = 1;

            // TODO : DB에 [일일 무료 가챠] 시간과 상태를 저장

            OnDailyGachaInfoChanged?.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 마지막 저장 시간을 기점으로 초기화가 되는 시점인지 확인하는 함수.
    /// 현재 시간이 마지막 저장 시간보다 미래면 true, 아니면 false.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    private bool IsDailyResetTime(DateTime date)
    {
        DateTime now = DateTime.Now;

        if (now.Year > date.Year && now.Hour >= date.Hour) return true;
        if (now.Year == date.Year && now.Month > date.Month && now.Hour >= date.Hour) return true;
        if (now.Year == date.Year && now.Month == date.Month && now.Day > date.Day && now.Hour >= date.Hour) return true;

        return false;
    }

    #endregion

    #region 12시간 광고 가챠 가능 여부 판정

    /// <summary>
    /// 광고 가챠가 가능한 상태인지 판별함.
    /// </summary>
    /// <returns></returns>
    public bool CanObtainAdGachaReward(GachaType type)
    {
        bool canObtain = false;

        switch (type)
        {
            case GachaType.Char:
                canObtain = CanObtainCharAdGachaReward();
                break;
            case GachaType.Stone:
                canObtain = CanObtainStoneAdGachaReward();
                break;
        }

        return canObtain;
    }

    private bool CanObtainCharAdGachaReward()
    {
        if (IsDailyCharAdGachaResetTime(out int stack))
        {
            _dailyCharAdGachaRewardInfo.state += stack;
            if (_dailyCharAdGachaRewardInfo.state > 2) _dailyCharAdGachaRewardInfo.state = 2;

            // TODO : DB에 시간과 상태를 저장

            OnDailyGachaInfoChanged?.Invoke();
            return true;
        }

        if (_dailyCharAdGachaRewardInfo.state >= 1) return true;

        return false;
    }

    private bool CanObtainStoneAdGachaReward()
    {
        if (IsDailyStoneAdGachaResetTime(out int stack))
        {
            _dailyStoneAdGachaRewardInfo.state += stack;
            if (_dailyStoneAdGachaRewardInfo.state > 2) _dailyStoneAdGachaRewardInfo.state = 2;

            // TODO : DB에 시간과 상태를 저장

            OnDailyGachaInfoChanged?.Invoke();
            return true;
        }

        if (_dailyStoneAdGachaRewardInfo.state >= 1) return true;

        return false;
    }

    /// <summary>
    /// 12시간 단위 누적 스택 계산
    /// </summary>
    private bool IsDailyCharAdGachaResetTime(out int stack)
    {
        DateTime now = DateTime.Now;
        DateTime lastTime = _dailyCharAdGachaRewardInfo.GetDateTime();
        TimeSpan difference = now - lastTime;

        stack = 0;

        if (difference.TotalHours >= 12)
        {
            int stackCount = (int)(difference.TotalHours / 12);
            stack = Mathf.Min(stackCount, 2); // 최대 2 스택

            // 마지막 갱신 시간 이동
            _dailyCharAdGachaRewardInfo.SetDateTime(lastTime.AddHours(12 * stack));

            Debug.Log($"스택 증가: {stack}, 새로운 마지막 갱신 시간: {_dailyCharAdGachaRewardInfo.GetDateTime()}");
            return true;
        }

        return false;
    }

    private bool IsDailyStoneAdGachaResetTime(out int stack)
    {
        DateTime now = DateTime.Now;
        DateTime lastTime = _dailyStoneAdGachaRewardInfo.GetDateTime();
        TimeSpan difference = now - lastTime;

        stack = 0;

        if (difference.TotalHours >= 12)
        {
            int stackCount = (int)(difference.TotalHours / 12);
            stack = Mathf.Min(stackCount, 2); // 최대 2 스택

            // 마지막 갱신 시간 이동
            _dailyStoneAdGachaRewardInfo.SetDateTime(lastTime.AddHours(12 * stack));

            Debug.Log($"스택 증가: {stack}, 새로운 마지막 갱신 시간: {_dailyStoneAdGachaRewardInfo.GetDateTime()}");
            return true;
        }

        return false;
    }

    #endregion

    #endregion
}