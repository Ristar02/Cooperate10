using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestPopup : MonoBehaviour
{
    [SerializeField] private QuestManager _questManager;

    [Header("List")]
    [SerializeField] private RectTransform _content;
    [SerializeField] private GameObject _QuestPrefab;

    [Header("UI")]
    [SerializeField] private TMP_Text _remainTime;
    [SerializeField] private Image _progressBar;
    [SerializeField] private Sprite _openedBox;
    [SerializeField] private Sprite _closedBox;

    [Header("Milestones")]
    [SerializeField] private Button[] _milestoneReward = new Button[5];
    private int[] _milestonePoints = { 20, 40, 60, 80, 100 };

    private DateTime _resetTime;
    private Coroutine _initRoutine;
    private Coroutine _countdownRoutine;


    private async void Start()
    {
        await Manager.DB.questDB.LoadUserQuestDataAsync();
        await Manager.DB.questDB.LoadUserMilestoneAsync();
        await Manager.DB.questDB.LoadDailyQuestStatesAsync();

        Init();
        _initRoutine = StartCoroutine(InitAndStartRoutine());
    }

    private void OnEnable()
    {
        Init();

        if (_initRoutine == null)
            _initRoutine = StartCoroutine(InitAndStartRoutine());
    }

    private void OnDisable()
    {
        // 타이머 정지
        if (_initRoutine != null)
        {
            StopCoroutine(_initRoutine);
            _initRoutine = null;
        }

        if (_countdownRoutine != null)
        {
            StopCoroutine(_countdownRoutine);
            _countdownRoutine = null;
        }
    }

    public void Init()
    {
        // 퀘스트 UI 갱신
        if (_content != null)
        {
            for (int i = _content.childCount - 1; i >= 0; i--)
            {
                Destroy(_content.GetChild(i).gameObject);
            }
        }

        foreach (var quest in _questManager._quests)
        {
            if (quest is IQuestView view)
            {
                GameObject questItem = Instantiate(_QuestPrefab, _content);
                QuestItem item = questItem.GetComponent<QuestItem>();
                item.Init(view);

                item.OnRewardReceived += () => Init();
            }
        }

        // 마일스톤 초기화
        InitMilestoneRewards();
        RefreshMilestoneRewards();

        _progressBar.fillAmount = (float)_questManager.UserQuestData.TotalPoint / _questManager.MaxPoint;
    }

    private void InitMilestoneRewards()
    {
        for (int i = 0; i < _milestoneReward.Length; i++)
        {
            if (_milestoneReward[i] == null) continue;

            int milestone = _milestonePoints[i];
            int index = i;

            _milestoneReward[index].onClick.RemoveAllListeners();
            _milestoneReward[index].onClick.AddListener(() =>
            {
                if (_milestoneReward[index].interactable)
                {
                    switch (index)
                    {
                        case 0:
                            _questManager.RewardGoldAsync(500);
                            _questManager.SetMilstoneState(index, true);
                            break;
                        case 1:
                            _questManager.RewardGoldAsync(700);
                            _questManager.SetMilstoneState(index, true);
                            break;
                        case 2:
                            _questManager.RewardGoldAsync(1000);
                            _questManager.SetMilstoneState(index, true);
                            break;
                        case 3:
                            _questManager.RewardGoldAsync(2000);
                            _questManager.SetMilstoneState(index, true);
                            break;
                        case 4:
                            _questManager.RewardGoldAsync(5000);
                            _questManager.RewardDiamondAsync(150);
                            _questManager.SetMilstoneState(index, true);
                            break;
                    }

                    _milestoneReward[index].image.sprite = _openedBox;
                    _milestoneReward[index].interactable = false;
                }
            });
        }
    }

    private void RefreshMilestoneRewards()
    {
        for (int i = 0; i < _milestoneReward.Length; i++)
        {
            if (_milestoneReward[i] == null) continue;

            int milestone = _milestonePoints[i];

            if (_questManager.UserQuestData.TotalPoint >= milestone)
            {
                if (_questManager.MilestoneData.MilestoneStates[i])
                {
                    _milestoneReward[i].interactable = false;
                    _milestoneReward[i].image.sprite = _openedBox;
                }
                else
                {
                    _milestoneReward[i].interactable = true;
                    _milestoneReward[i].image.sprite = _closedBox;
                }
            }
            else
            {
                _milestoneReward[i].interactable = false;
                _milestoneReward[i].image.sprite = _closedBox;
            }
        }
    }

    private IEnumerator InitAndStartRoutine()
    {
        Task<DateTime> task = GetKstNowAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        DateTime kstNow = task.Result;
        DateTime resetTime = new DateTime(kstNow.Year, kstNow.Month, kstNow.Day, 6, 0, 0);
        //DateTime resetTime = new DateTime(kstNow.Year, kstNow.Month, kstNow.Day, 7, 04, 00);

        if (kstNow >= resetTime)
        {
            resetTime = resetTime.AddDays(1);
        }

        _resetTime = resetTime;
        _countdownRoutine = StartCoroutine(UpdateRemainRoutine());
    }

    private IEnumerator UpdateRemainRoutine()
    {
        while (true)
        {
            TimeSpan remain = _resetTime - DateTime.Now;

            if (remain <= TimeSpan.Zero)
            {
                Debug.Log($"{DateTime.Now} 퀘스트 초기화 시간 / 서버 시간 재동기화");

                // 퀘스트 초기화
                QuestManager.Instance.ResetAllQuests();
                Init();

                _initRoutine = StartCoroutine(InitAndStartRoutine());
                yield break;
            }

            _remainTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", remain.Hours, remain.Minutes, remain.Seconds);

            yield return new WaitForSeconds(1f);
        }
    }

    private async Task<DateTime> GetKstNowAsync()
    {
        long serverTime = await Manager.DB.LoadSeverTimeAsync();

        DateTime utcNow = DateTimeOffset.FromUnixTimeMilliseconds(serverTime).UtcDateTime;
        DateTime kstNow = utcNow.AddHours(9);

        return kstNow;
    }
}
