using System;

[Serializable]
public class UserQuestData
{
    public string DayKey;
    public int TotalPoint;

    public UserQuestData(string dayKey, int totalPoint)
    {
        DayKey = dayKey;
        TotalPoint = totalPoint;
    }
}

[Serializable]
public class MilestoneData
{
    public bool[] MilestoneStates = new bool[5];

    public MilestoneData(bool[] milestoneStates)
    {
        MilestoneStates = milestoneStates;
    }
}

 [Serializable]
public class DailyQuestState
{
    public bool IsCompleted;    
    public bool IsReceived;    
}