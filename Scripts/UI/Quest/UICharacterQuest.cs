using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterQuest : UISelectionEntry<CharacterQuest>
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
    public UIQuestTask uiQuestTaskPrefab;
    public UICharacterItem uiRewardItemPrefab;
    public Transform uiQuestTaskContainer;
    public Transform uiRewardItemContainer;
    public GameObject questDoneObject;

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

    protected override void UpdateData()
    {
        var characterQuest = Data;
        var quest = characterQuest.GetQuest();

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, quest == null ? "Unknow" : quest.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, quest == null ? "N/A" : quest.description);

        if (textRewardGold != null)
            textRewardGold.text = string.Format(rewardGoldFormat, quest == null ? "0" : quest.rewardGold.ToString("N0"));

        if (textRewardExp != null)
            textRewardExp.text = string.Format(rewardExpFormat, quest == null ? "0" : quest.rewardExp.ToString("N0"));

        if (questDoneObject != null)
            questDoneObject.SetActive(characterQuest.isDone);

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
