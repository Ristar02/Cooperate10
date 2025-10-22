
public interface IQuestView 
{
    bool IsComplete { get; }
    bool IsReceive { get; set; }
    int QuestID { get; }
    QuestType QuestType { get; }
    string QuestDesc { get; }
    int RewardPoint { get; }
    int MaxProgress { get; }
    int CurProgress { get; }
}
