using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestTaskType : byte
{
    KillMonster,
    CollectItem,
}

[System.Serializable]
public struct QuestTask
{
    public QuestTaskType taskType;
    [StringShowConditional(conditionFieldName: "taskType", conditionValue: "KillMonster")]
    public MonsterCharacterAmount monsterCharacterAmount;
    [StringShowConditional(conditionFieldName: "taskType", conditionValue: "CollectItem")]
    public ItemAmount itemAmount;
}

[CreateAssetMenu(fileName = "Quest", menuName = "Create GameData/Quest")]
public class Quest : BaseGameData
{
    public QuestTask[] tasks;
    public int rewardGold;
    public int rewardExp;
    public ItemAmount[] rewardItems;
    private HashSet<string> cacheKillMonsterIds;
    public HashSet<string> CacheKillMonsterIds
    {
        get
        {
            if (cacheKillMonsterIds == null)
            {
                cacheKillMonsterIds = new HashSet<string>();
                foreach (var task in tasks)
                {
                    if (task.taskType == QuestTaskType.KillMonster &&
                        task.monsterCharacterAmount.monster != null &&
                        task.monsterCharacterAmount.amount > 0)
                        cacheKillMonsterIds.Add(task.monsterCharacterAmount.monster.Id);
                }
            }
            return cacheKillMonsterIds;
        }
    }
}
