using UnityEngine;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    public partial class UICharacterQuest : UIDataForCharacter<CharacterQuest>
    {
        public CharacterQuest CharacterQuest { get { return Data; } }
        public Quest Quest { get { return CharacterQuest != null ? CharacterQuest.GetQuest() : null; } }

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
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public TextWrapper uiTextRewardExp;
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
            Profiler.BeginSample("UICharacterQuest - Update UI");
            Quest quest = !Data.IsEmpty() ? Data.GetQuest() : null;

            if (quest != null && showQuestTaskList)
            {
                CacheQuestTaskList.Generate(quest.tasks, (index, task, ui) =>
                {
                    UIQuestTask uiQuestTask = ui.GetComponent<UIQuestTask>();
                    bool isComplete = false;
                    int progress = Data.GetProgress(Character, index, out isComplete);
                    uiQuestTask.Data = new QuestTaskProgressTuple(task, progress);
                    uiQuestTask.Show();
                });
            }
            Profiler.EndSample();
        }

        protected override void UpdateData()
        {
            bool isComplete = CharacterQuest.isComplete;
            bool isAllTasksDone = CharacterQuest.IsAllTasksDone(Character);

            string titleFormat = isComplete ? questCompleteTitleFormat : (isAllTasksDone ? questTasksCompleteTitleFormat : questOnGoingTitleFormat);

            if (uiTextTitle != null)
                uiTextTitle.text = string.Format(titleFormat, Quest == null ? LanguageManager.GetUnknowTitle() : Quest.Title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, Quest == null ? LanguageManager.GetUnknowDescription() : Quest.Description);

            if (uiTextRewardExp != null)
                uiTextRewardExp.text = string.Format(rewardExpFormat, Quest == null ? "0" : Quest.rewardExp.ToString("N0"));

            if (uiTextRewardGold != null)
                uiTextRewardGold.text = string.Format(rewardGoldFormat, Quest == null ? "0" : Quest.rewardGold.ToString("N0"));

            if (Quest != null && showRewardItemList)
            {
                CacheRewardItemList.Generate(Quest.rewardItems, (index, rewardItem, ui) =>
                {
                    CharacterItem characterItem = CharacterItem.Create(rewardItem.item);
                    characterItem.amount = rewardItem.amount;
                    UICharacterItem uiCharacterItem = ui.GetComponent<UICharacterItem>();
                    uiCharacterItem.Setup(new CharacterItemTuple(characterItem, characterItem.level, InventoryType.NonEquipItems), null, -1);
                    uiCharacterItem.Show();
                });
            }

            if (uiRewardItemRoot != null)
                uiRewardItemRoot.SetActive(showRewardItemList && Quest.rewardItems.Length > 0);

            if (uiQuestTaskRoot != null)
                uiQuestTaskRoot.SetActive(showQuestTaskList && Quest.tasks.Length > 0);

            if (questCompleteStatusObject != null)
                questCompleteStatusObject.SetActive(isComplete);

            if (questTasksCompleteStatusObject != null)
                questTasksCompleteStatusObject.SetActive(!isComplete && isAllTasksDone);

            if (questOnGoingStatusObject != null)
                questOnGoingStatusObject.SetActive(!isComplete && !isAllTasksDone);
        }
    }
}
