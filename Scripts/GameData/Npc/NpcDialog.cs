using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NpcDialogType : byte
{
    Normal,
    Quest,
}

public enum NpcDialogConditionType : byte
{
    LevelMoreThanOrEqual,
    LevelLessThanOrEqual,
    QuestNotStarted,
    QuestTasksNotCompleted,
    QuestTasksCompleted,
    QuestCompleted,
}

[System.Serializable]
public struct NpcDialogCondition
{
    public NpcDialogConditionType conditionType;
    [StringShowConditional(conditionFieldName: "conditionType", conditionValues: new string[] { "QuestNotStarted", "QuestTasksNotFinished", "QuestTasksFinished", "QuestFinished" })]
    public Quest quest;
    [StringShowConditional(conditionFieldName: "conditionType", conditionValues: new string[] { "LevelMoreThan", "LevelLessThan" })]
    public int conditionalLevel;
    public bool IsPass(IPlayerCharacterData character)
    {
        var indexOfQuest = -1;
        var questTasksCompleted = false;
        var questCompleted = false;
        if (quest != null)
        {
            indexOfQuest = character.IndexOfQuest(quest.Id);
            if (indexOfQuest >= 0)
            {
                var characterQuest = character.Quests[indexOfQuest];
                questTasksCompleted = characterQuest.IsAllTasksDone(character);
                questCompleted = characterQuest.isComplete;
            }
        }
        switch (conditionType)
        {
            case NpcDialogConditionType.LevelMoreThanOrEqual:
                return character.Level >= conditionalLevel;
            case NpcDialogConditionType.LevelLessThanOrEqual:
                return character.Level <= conditionalLevel;
            case NpcDialogConditionType.QuestNotStarted:
                return indexOfQuest < 0;
            case NpcDialogConditionType.QuestTasksNotCompleted:
                return !questTasksCompleted;
            case NpcDialogConditionType.QuestTasksCompleted:
                return questTasksCompleted;
            case NpcDialogConditionType.QuestCompleted:
                return questCompleted;
        }
        return true;
    }
}

[System.Serializable]
public struct NpcDialogMenu
{
    public string title;
    public NpcDialogCondition[] showConditions;
    public bool isCloseMenu;
    [BoolShowConditional(conditionFieldName: "isCloseMenu", conditionValue: false)]
    public NpcDialog dialog;

    public bool IsPassConditions(IPlayerCharacterData character)
    {
        foreach (var showCondition in showConditions)
        {
            if (!showCondition.IsPass(character))
                return false;
        }
        return true;
    }
}

[CreateAssetMenu(fileName = "NpcDialog", menuName = "Create GameData/NpcDialog")]
public class NpcDialog : BaseGameData
{
    public NpcDialogType type;
    [StringShowConditional(conditionFieldName: "type", conditionValue: "Quest")]
    public Quest quest;
    [StringShowConditional(conditionFieldName: "type", conditionValue: "Normal")]
    public NpcDialogMenu[] menus;
}
