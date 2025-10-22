using System;

/// <summary>
/// 모든 QuestDataSO가 구현해야 하는 인터페이스
/// </summary>
public interface IQuest
{
    QuestType QuestType { get; }

    public void Subscribe(Action onClearCondition);

    public void Unsubscribe(Action onClearCondition);

    public void ResetProgress();
}
