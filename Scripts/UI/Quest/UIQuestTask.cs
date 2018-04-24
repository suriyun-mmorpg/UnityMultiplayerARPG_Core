using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestTask : UISelectionEntry<KeyValuePair<QuestTask, int>>
{
    [Header("Generic Info Format")]
    [Tooltip("Kill Monster Task Format => {0} = {Title}, {1} = {Progress}, {2} = {Amount}")]
    public string killMonsterTaskFormat = "Kills {0}: {1}/{2}";
    [Tooltip("Collect Item Task Format => {0} = {Title}, {1} = {Progress}, {2} = {Amount}")]
    public string collectItemTaskFormat = "Collects {0}: {1}/{2}";

    [Header("UI Elements")]
    public Text taskDescription;
    public GameObject taskCompleteObject;

    protected override void UpdateData()
    {
        var task = Data.Key;
        var progress = Data.Value;
        var isComplete = false;
        switch (task.taskType)
        {
            case QuestTaskType.KillMonster:
                var monsterCharacterAmount = task.monsterCharacterAmount;
                var monsterTitle = monsterCharacterAmount.monster == null ? "Unknow" : monsterCharacterAmount.monster.title;
                var monsterKillAmount = monsterCharacterAmount.amount;
                isComplete = progress >= monsterKillAmount;
                if (taskDescription != null)
                    taskDescription.text = string.Format(killMonsterTaskFormat, monsterTitle, progress.ToString("N0"), monsterKillAmount.ToString("N0"));
                break;
            case QuestTaskType.CollectItem:
                var itemAmount = task.itemAmount;
                var itemTitle = itemAmount.item == null ? "Unknow" : itemAmount.item.title;
                var itemCollectAmount = itemAmount.amount;
                isComplete = progress >= itemCollectAmount;
                if (taskDescription != null)
                    taskDescription.text = string.Format(killMonsterTaskFormat, itemTitle, progress.ToString("N0"), itemCollectAmount.ToString("N0"));
                break;
        }
        if (taskCompleteObject != null)
            taskCompleteObject.SetActive(isComplete);
    }
}
