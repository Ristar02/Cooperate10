using System;
using UnityEngine;

public enum QuestType
{
    Battle,
    Item,
    State
}

/// <summary>
/// 퀘스트 조건 클래스
/// - IQuestCondition 인터페이스를 구현하며 제네릭 타입 T를 사용하여 다양한 조건을 처리할 수 있음
/// - 조건 클리어 시 OnClearCondition 이벤트를 통해 추후에 처리할 로직을 연결
/// </summary>
/// <typeparam name="T"></typeparam>
public class QuestDataSO<T> : ScriptableObject, IQuest, IQuestView
{
    [Header("일퀘 클리어 여부")]
    [SerializeField] bool _isCompleted;
    [SerializeField] bool _isReceived;
    public bool IsComplete { get { return _isCompleted; } set { _isCompleted = value; } }
    public bool IsReceive { get { return _isReceived; } set { _isReceived = value; } }

    [Header("퀘스트 데이터")]
    [SerializeField] int _questID;
    public int QuestID { get { return _questID; } }

    [SerializeField] QuestType _questType;
    public QuestType QuestType { get { return _questType; } }

    [SerializeField] string _questDesc; // 퀘스트 설명
    public string QuestDesc { get { return _questDesc; } }

    [SerializeField] int _rewardPoint; // 퀘스트 보상 포인트
    public int RewardPoint { get { return _rewardPoint; } }

    [SerializeField] int _maxProgress; // 퀘스트 달성 조건
    public int MaxProgress { get { return _maxProgress; } }

    [SerializeField] int _curProgress; // 현재 진행도
    public int CurProgress { get { return _curProgress; } set { _curProgress = value; } }

    public Action OnClearCondition;

    public bool IsClear()
    {
        return _isCompleted;
    }

    public void ResetProgress()
    {
        _curProgress = 0;
        _isCompleted = false;
        _isReceived = false;
    }

    public void ClearQuest()
    {
        _curProgress++;
        _isCompleted = true;
    }

    public void Subscribe(Action onClearCondition)
    {
        this.OnClearCondition += onClearCondition;
    }

    public void Unsubscribe(Action onClearCondition)
    {
        this.OnClearCondition -= onClearCondition;
    }

    /// <summary>
    /// 특정 조건이 클리어 되었을 때 호출되는 메서드
    /// Override하여 특정 조건에 대한 로직을 구현
    /// </summary>
    /// <param name="value">condition</param>
    public virtual void ClearCondition(T value) { }

    public async virtual void Clear()
    {
        //OnClearCondition?.Invoke();

        if (_curProgress >= _maxProgress)
        {
            Debug.Log($"{_questDesc} 완료");
            return;
        }
        _curProgress++;

        if (_curProgress == _maxProgress)
        {
            _isCompleted = true;
        }
        await Manager.DB.questDB.SaveQuestCompletedAsync(_questID, _isCompleted);
    }
}
