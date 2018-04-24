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
    QuestNotStarted,
    QuestTasksNotFinished,
    QuestTasksFinished,
    QuestFinished,
}

[System.Serializable]
public struct NpcDialogCondition
{
    public NpcDialogConditionType conditionType;
    [StringShowConditional(conditionFieldName: "conditionType", conditionValues: new string[] { "QuestNotStarted", "QuestTasksNotFinished", "QuestTasksFinished", "QuestFinished" })]
    public Quest quest;
}

[System.Serializable]
public struct NpcDialogMenu
{
    public string title;
    public NpcDialogCondition[] showConditions;
    public bool isCloseMenu;
    [BoolShowConditional(conditionFieldName: "isCloseMenu", conditionValue: false)]
    public NpcDialog dialog;

    public bool IsPassConditions(ICharacterData character)
    {
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
