using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    //  모든 퀘스트 데이터
    [SerializeField] public List<ScriptableObject> _quests;
    [SerializeField] public StateQuest loginQuest;
    [SerializeField] public ItemQuest adWatchQuest;

    public static QuestManager Instance { get; private set; }

    private int _maxPoint = 100;
    public int MaxPoint { get { return _maxPoint; } }

    private MilestoneData _milestoneData = new MilestoneData(new bool[5]);
    public MilestoneData MilestoneData => _milestoneData;

    private UserQuestData _userQuestData;
    public UserQuestData UserQuestData => _userQuestData;


    private void Awake()
    {
        SetSingleton();

        Subscribe();
    }

    private void Start()
    {
        OnStateChanged(loginQuest, true);
    }

    private void OnDestroy()
    {
        UnSubscribe();
    }

    #region Init

    //  (이벤트 연결 및 해제 / 싱글톤 세팅)
    private void SetSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Subscribe()
    {
        foreach (var questObj in _quests)
        {
            if (questObj is IQuest condition)
            {
                condition.Subscribe(() => ClearHandler(condition));
            }
        }
    }

    private void UnSubscribe()
    {
        foreach (var questObj in _quests)
        {
            if (questObj is IQuest condition)
            {
                condition.Unsubscribe(() => ClearHandler(condition));
            }
        }
    }

    #endregion

    private void ClearHandler(IQuest condition)
    {
        if (condition is QuestDataSO<object> quest)
        {
            if (!quest.IsComplete)
            {
                quest.IsComplete = true;
                //Manager.DB.questDB.SaveQuestCompletedAsync(quest.QuestID, quest.IsComplete);
            }
        }
    }

    public void CallHandler<T>(QuestDataSO<T> condition, T value)
    {
        condition.ClearCondition(value);
    }

    /// <summary>
    /// 모든 퀘스트 진행 상태를 초기화하는 메서드
    /// </summary>
    public void ResetAllQuests()
    {
        foreach (var questObj in _quests)
        {
            if (questObj is IQuest quest)
            {
                quest.ResetProgress();
            }
        }

        _userQuestData.TotalPoint = 0;

        for (int i = 0; i < _milestoneData.MilestoneStates.Length; i++)
        {
            _milestoneData.MilestoneStates[i] = false;
        }

        Manager.DB.questDB.DeleteUserQuestDataAsync();

        // 로그인 퀘스트
        OnStateChanged(loginQuest, true);
    }

    /// <summary>
    /// 퀘스트 완료 시 보상을 지급하고 유저의 TotalPoint를 갱신하는 메서드
    /// </summary>
    public void ReceiveReward(IQuestView condition)
    {
        if (!condition.IsReceive && condition.IsComplete)
        {
            condition.IsReceive = true;

            if (_userQuestData.TotalPoint < _maxPoint)
            {
                _userQuestData.TotalPoint += condition.RewardPoint;
                Manager.DB.questDB.SaveUserQuestDataAsync();
                Manager.DB.questDB.SaveQuestReceivedAsync(condition.QuestID, true);
            }
            else
            {
                _userQuestData.TotalPoint = _maxPoint;
            }
        }
    }

    /// <summary>
    /// 지정한 양의 골드를 유저 DB에 저장하는 메서드
    /// </summary>
    /// <param name="amount">지급할 gold 양</param>
    public async void RewardGoldAsync(int amount)
    {
        await Manager.DB.AddGoldAsync(amount);
    }

    /// <summary>
    /// 지정한 양의 다이아를 유저 DB에 저장하는 메서드
    /// </summary>
    /// <param name="amount">지급할 Diamond 양</param>
    public async void RewardDiamondAsync(int amount)
    {
        await Manager.DB.AddDiamondAsync(amount);
    }

    /// <summary>
    /// 지정한 Milestone 보상 수령 상태를 설정하고 DB에 저장하는 메서드
    /// </summary>
    /// <param name="index">Milestone 인덱스</param>
    /// <param name="isRewardReceived">보상 수령 여부</param>
    public void SetMilstoneState(int index, bool isRewardReceived)
    {
        _milestoneData.MilestoneStates[index] = isRewardReceived;
        Manager.DB.questDB.SaveDailyRewardAsync(index, true);
    }

    /// <summary>
    /// 유저 퀘스트 데이터 DB -> 인게임 적용하는 메서드
    /// </summary>
    public void ApplyUserQuestData(UserQuestData data)
    {
        _userQuestData = data;
    }

    /// <summary>
    /// 유저 Milestone 데이터 DB -> 인게임 적용하는 메서드
    /// </summary>
    public void ApplyMilestoneData(MilestoneData data)
    {
        _milestoneData = data;
    }

    public void ApplyDailyQuestStates(Dictionary<int, DailyQuestState> states)
    {
        foreach (var questObj in _quests)
        {
            if (questObj is QuestDataSO<object> quest)
            {
                if (states.TryGetValue(quest.QuestID, out DailyQuestState state))
                {
                    quest.IsComplete = state.IsCompleted;
                    quest.IsReceive = state.IsReceived;
                }
            }
        }
    }

    #region 각 조건 클리어 시 호출할 함수

    public void OnMonsterKilled(BattleQuest condition, int monsterID)
    {
        CallHandler<int>(condition, monsterID);
    }

    public void OnBossKilled(BattleQuest condition, int bossID)
    {
        CallHandler<int>(condition, bossID);
    }

    public void OnItemCollected(ItemQuest condition, bool useItem)
    {
        CallHandler<bool>(condition, useItem);
    }

    public void OnItemUsed(ItemQuest condition, bool useItem)
    {
        CallHandler<bool>(condition, useItem);
    }

    public void OnAdWatched(ItemQuest condition, bool adWatched)
    {
        CallHandler<bool>(condition, adWatched);
    }

    public void OnStateChanged(StateQuest condition, bool state)
    {
        CallHandler<bool>(condition, state);
    }
    #endregion
}
