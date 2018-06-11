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
    QuestOngoing,
    QuestTasksCompleted,
    QuestCompleted,
}

[System.Serializable]
public struct NpcDialogCondition
{
    public NpcDialogConditionType conditionType;
    [StringShowConditional(conditionFieldName: "conditionType", conditionValues: new string[] { "QuestNotStarted", "QuestOngoing", "QuestTasksCompleted", "QuestCompleted" })]
    public Quest quest;
    [StringShowConditional(conditionFieldName: "conditionType", conditionValues: new string[] { "LevelMoreThanOrEqual", "LevelLessThanOrEqual" })]
    public int conditionalLevel;
    public bool IsPass(IPlayerCharacterData character)
    {
        var indexOfQuest = -1;
        var questTasksCompleted = false;
        var questCompleted = false;
        if (quest != null)
        {
            indexOfQuest = character.IndexOfQuest(quest.HashId);
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
            case NpcDialogConditionType.QuestOngoing:
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
        if (dialog != null && dialog.type == NpcDialogType.Quest)
        {
            if (dialog.quest == null)
                return false;
            var indexOfQuest = character.IndexOfQuest(dialog.quest.HashId);
            if (indexOfQuest >= 0 && character.Quests[indexOfQuest].isComplete)
                return false;
        }
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
    public const int QUEST_ACCEPT_MENU_INDEX = 0;
    public const int QUEST_DECLINE_MENU_INDEX = 1;
    public const int QUEST_ABANDON_MENU_INDEX = 2;
    public const int QUEST_COMPLETE_MENU_INDEX = 3;

    public NpcDialogType type;
    public NpcDialogMenu[] menus;
    public Quest quest;
    public NpcDialog questAcceptedDialog;
    public NpcDialog questDeclinedDialog;
    public NpcDialog questAbandonedDialog;
    public NpcDialog questCompletedDailog;
}
