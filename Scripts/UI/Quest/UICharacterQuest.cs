using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterQuest : UIDataForCharacter<CharacterQuest>
    {
        [Header("Generic Info Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string questOnGoingTitleFormat = "{0} (Ongoing)";
        [Tooltip("Title Format => {0} = {Title}")]
        public string questTasksCompleteTitleFormat = "{0} (Task Completed)";
        [Tooltip("Title Format => {0} = {Title}")]
        public string questCompleteTitleFormat = "{0} (Completed)";
        [Tooltip("Description Format => {0} = {Description}")]
        public string descriptionFormat = "{0}";
        [Tooltip("Reward Exp Format => {0} = {Exp}")]
        public string rewardExpFormat = "Reward Exp: {0}";
        [Tooltip("Reward Gold Format => {0} = {Amount}")]
        public string rewardGoldFormat = "Reward Gold: {0}";

        [Header("UI Elements")]
        public Text textTitle;
        public TextWrapper uiTextTitle;
        public Text textDescription;
        public TextWrapper uiTextDescription;
        public Text textRewardExp;
        public TextWrapper uiTextRewardExp;
        public Text textRewardGold;
        public TextWrapper uiTextRewardGold;
        [Header("Reward Items")]
        public bool showRewardItemList;
        public GameObject uiRewardItemRoot;
        public UICharacterItem uiRewardItemPrefab;
        public Transform uiRewardItemContainer;
        [Header("Quest Tasks")]
        public bool showQuestTaskList;
        public GameObject uiQuestTaskRoot;
        public UIQuestTask uiQuestTaskPrefab;
        public Transform uiQuestTaskContainer;
        [Header("Quest Status")]
        public GameObject questOnGoingStatusObject;
        public GameObject questTasksCompleteStatusObject;
        public GameObject questCompleteStatusObject;

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

        protected override void UpdateUI()
        {
            var characterQuest = Data;
            var quest = !characterQuest.IsEmpty() ? characterQuest.GetQuest() : null;

            if (quest != null && showQuestTaskList)
            {
                CacheQuestTaskList.Generate(quest.tasks, (index, task, ui) =>
                {
                    var uiQuestTask = ui.GetComponent<UIQuestTask>();
                    var isComplete = false;
                    var progress = characterQuest.GetProgress(character, index, out isComplete);
                    uiQuestTask.Data = new QuestTaskProgressTuple(task, progress);
                    uiQuestTask.Show();
                });
            }
        }

        protected override void UpdateData()
        {
            MigrateUIComponents();

            var characterQuest = Data;
            var quest = characterQuest.GetQuest();

            var isComplete = characterQuest.isComplete;
            var isAllTasksDone = characterQuest.IsAllTasksDone(character);

            var titleFormat = isComplete ? questCompleteTitleFormat : (isAllTasksDone ? questTasksCompleteTitleFormat : questOnGoingTitleFormat);

            if (uiTextTitle != null)
                uiTextTitle.text = string.Format(titleFormat, quest == null ? "Unknow" : quest.title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, quest == null ? "N/A" : quest.description);

            if (uiTextRewardExp != null)
                uiTextRewardExp.text = string.Format(rewardExpFormat, quest == null ? "0" : quest.rewardExp.ToString("N0"));

            if (uiTextRewardGold != null)
                uiTextRewardGold.text = string.Format(rewardGoldFormat, quest == null ? "0" : quest.rewardGold.ToString("N0"));

            if (quest != null && showRewardItemList)
            {
                CacheRewardItemList.Generate(quest.rewardItems, (index, rewardItem, ui) =>
                {
                    var characterItem = CharacterItem.Create(rewardItem.item);
                    characterItem.amount = rewardItem.amount;
                    var uiCharacterItem = ui.GetComponent<UICharacterItem>();
                    uiCharacterItem.Setup(new CharacterItemLevelTuple(characterItem, characterItem.level), null, -1, string.Empty);
                    uiCharacterItem.Show();
                });
            }

            if (uiRewardItemRoot != null)
                uiRewardItemRoot.SetActive(showRewardItemList && quest.rewardItems.Length > 0);

            if (uiQuestTaskRoot != null)
                uiQuestTaskRoot.SetActive(showQuestTaskList && quest.tasks.Length > 0);

            if (questCompleteStatusObject != null)
                questCompleteStatusObject.SetActive(isComplete);

            if (questTasksCompleteStatusObject != null)
                questTasksCompleteStatusObject.SetActive(!isComplete && isAllTasksDone);

            if (questOnGoingStatusObject != null)
                questOnGoingStatusObject.SetActive(!isComplete && !isAllTasksDone);
        }

        [ContextMenu("Migrate UI Components")]
        public void MigrateUIComponents()
        {
            uiTextTitle = MigrateUIHelpers.SetWrapperToText(textTitle, uiTextTitle);
            uiTextDescription = MigrateUIHelpers.SetWrapperToText(textDescription, uiTextDescription);
            uiTextRewardExp = MigrateUIHelpers.SetWrapperToText(textRewardExp, uiTextRewardExp);
            uiTextRewardGold = MigrateUIHelpers.SetWrapperToText(textRewardGold, uiTextRewardGold);
        }
    }
}
