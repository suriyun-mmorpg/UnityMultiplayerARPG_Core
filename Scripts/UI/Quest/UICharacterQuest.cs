using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterQuest : UIDataForCharacter<CharacterQuest>
{
    [Header("Generic Info Format")]
    [Tooltip("Title Format => {0} = {Title}")]
    public string titleFormat = "{0}";
    [Tooltip("Description Format => {0} = {Description}")]
    public string descriptionFormat = "{0}";
    [Tooltip("Reward Gold Format => {0} = {Amount}")]
    public string rewardGoldFormat = "{0}";
    [Tooltip("Reward Exp Format => {0} = {Exp}")]
    public string rewardExpFormat = "{0}";

    [Header("UI Elements")]
    public Text textTitle;
    public Text textDescription;
    public Text textRewardGold;
    public Text textRewardExp;
    public GameObject questCompleteObject;
    public GameObject allTasksCompleteObject;
    public bool showRewardItemList;
    public UICharacterItem uiRewardItemPrefab;
    public Transform uiRewardItemContainer;
    public bool showQuestTaskList;
    public UIQuestTask uiQuestTaskPrefab;
    public Transform uiQuestTaskContainer;

    private UIList cacheRewardItemList;
    public UIList CacheRewardItemList
    {
        get
        {
            if (cacheRewardItemList == null)
            {
                cacheRewardItemList = gameObject.AddComponent<UIList>();
                cacheRewardItemList.uiPrefab = uiRewardItemPrefab.gameObject;
                cacheRewardItemList.uiContainer = uiRewardItemContainer;
            }
            return cacheRewardItemList;
        }
    }

    private UIList cacheQuestTaskList;
    public UIList CacheQuestTaskList
    {
        get
        {
            if (cacheQuestTaskList == null)
            {
                cacheQuestTaskList = gameObject.AddComponent<UIList>();
                cacheQuestTaskList.uiPrefab = uiQuestTaskPrefab.gameObject;
                cacheQuestTaskList.uiContainer = uiQuestTaskContainer;
            }
            return cacheQuestTaskList;
        }
    }

    protected void Update()
    {
        var characterQuest = Data;
        var quest = characterQuest.GetQuest();

        if (quest != null && showQuestTaskList)
        {
            CacheQuestTaskList.Generate(quest.tasks, (index, task, ui) =>
            {
                var uiQuestTask = ui.GetComponent<UIQuestTask>();
                var isComplete = false;
                uiQuestTask.Data = new KeyValuePair<QuestTask, int>(task, characterQuest.GetProgress(character, index, out isComplete));
                uiQuestTask.Show();
            });
        }
    }

    protected override void UpdateData()
    {
        var characterQuest = Data;
        var quest = characterQuest.GetQuest();
        var owningCharacter = BasePlayerCharacterController.OwningCharacter;

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, quest == null ? "Unknow" : quest.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, quest == null ? "N/A" : quest.description);

        if (textRewardGold != null)
            textRewardGold.text = string.Format(rewardGoldFormat, quest == null ? "0" : quest.rewardGold.ToString("N0"));

        if (textRewardExp != null)
            textRewardExp.text = string.Format(rewardExpFormat, quest == null ? "0" : quest.rewardExp.ToString("N0"));

        if (questCompleteObject != null)
            questCompleteObject.SetActive(characterQuest.isComplete);

        if (allTasksCompleteObject != null)
            allTasksCompleteObject.SetActive(characterQuest.IsAllTasksDone(owningCharacter));

        if (quest != null && showRewardItemList)
        {
            CacheRewardItemList.Generate(quest.rewardItems, (index, rewardItem, ui) =>
            {
                var characterItem = CharacterItem.Create(rewardItem.item, 1);
                characterItem.amount = rewardItem.amount;
                var uiCharacterItem = ui.GetComponent<UICharacterItem>();
                uiCharacterItem.Setup(new KeyValuePair<CharacterItem, int>(characterItem, characterItem.level), null, -1, string.Empty);
                uiCharacterItem.Show();
            });
        }
    }
}

[System.Serializable]
public class UICharacterQuestEvent : UnityEvent<UICharacterQuest> { }
