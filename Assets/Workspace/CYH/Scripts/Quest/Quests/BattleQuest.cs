using UnityEngine;

[CreateAssetMenu(fileName = "New Battle Quest", menuName = "Quests/Battle Quest")]
public class BattleQuest : QuestDataSO<int>
{
    [SerializeField] private int _targetMonsterID;

    public override void ClearCondition(int monsterID)
    {
        if (monsterID == _targetMonsterID)
        {
            Clear();
        }
    }
}
