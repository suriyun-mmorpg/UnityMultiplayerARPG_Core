using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public NpcDialog dialog;
}

[System.Serializable]
public struct NpcDialogMenu
{
    public string title;
    public bool isCloseMenu;
    [BoolShowConditional(conditionFieldName: "isCloseMenu", conditionValue: false)]
    public NpcDialog dialog;
}

[CreateAssetMenu(fileName = "NpcDialog", menuName = "Create GameData/NpcDialog")]
public class NpcDialog : BaseGameData
{
    public NpcDialogCondition[] dialogConditions;
    public NpcDialogMenu[] menus;
}
