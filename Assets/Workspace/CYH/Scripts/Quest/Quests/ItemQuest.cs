using UnityEngine;

[CreateAssetMenu(fileName = "New Item Quest", menuName = "Quests/Item Quest")]
public class ItemQuest : QuestDataSO<bool>
{
    public override void ClearCondition(bool useItem)
    {
        if (useItem)
        {
            Clear();
        }
    }
}
