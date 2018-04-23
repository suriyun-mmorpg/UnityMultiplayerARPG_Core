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
    public MonsterCharacterAmountPair monsterCharacterAmount;
    [StringShowConditional(conditionFieldName: "taskType", conditionValue: "CollectItem")]
    public ItemAmountPair itemAmount;
}

[CreateAssetMenu(fileName = "Quest", menuName = "Create GameData/Quest")]
public class Quest : BaseGameData
{
    public QuestTask[] tasks;
}
