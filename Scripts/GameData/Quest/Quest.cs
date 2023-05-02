using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum QuestTaskType : byte
    {
        KillMonster,
        CollectItem,
        TalkToNpc,
        Custom = 254,
    }

    [CreateAssetMenu(fileName = GameDataMenuConsts.QUEST_FILE, menuName = GameDataMenuConsts.QUEST_MENU, order = GameDataMenuConsts.QUEST_ORDER)]
    public partial class Quest : BaseGameData
    {
        [Category("Quest Settings")]
        [Tooltip("Requirement to receive quest")]
        public QuestRequirement requirement = default(QuestRequirement);
        public QuestTask[] tasks = new QuestTask[0];
        [Tooltip("Quests which will be abandoned when accept this quest")]
        public Quest[] abandonQuests = new Quest[0];
        [Header("Rewarding")]
        public bool resetAttributes;
        public bool resetSkills;
        public PlayerCharacter changeCharacterClass;
        public Faction changeCharacterFaction;
        public int rewardExp = 0;
        public int rewardGold = 0;
        public int rewardStatPoints = 0;
        public int rewardSkillPoints = 0;
        [ArrayElementTitle("currency")]
        public CurrencyAmount[] rewardCurrencies = new CurrencyAmount[0];
        [ArrayElementTitle("item")]
        public ItemAmount[] rewardItems = new ItemAmount[0];
        [ArrayElementTitle("item")]
        public ItemAmount[] selectableRewardItems = new ItemAmount[0];
        [ArrayElementTitle("item")]
        public ItemRandomByWeight[] randomRewardItems = new ItemRandomByWeight[0];
        [Tooltip("If this is `TRUE` character will be able to do this quest repeatedly")]
        public bool canRepeat;

        [System.NonSerialized]
        private HashSet<int> _cacheKillMonsterIds;
        public HashSet<int> CacheKillMonsterIds
        {
            get
            {
                if (_cacheKillMonsterIds == null)
                {
                    _cacheKillMonsterIds = new HashSet<int>();
                    foreach (QuestTask task in tasks)
                    {
                        if (task.taskType == QuestTaskType.KillMonster &&
                            task.monsterCharacterAmount.monster != null &&
                            task.monsterCharacterAmount.amount > 0)
                            _cacheKillMonsterIds.Add(task.monsterCharacterAmount.monster.DataId);
                    }
                }
                return _cacheKillMonsterIds;
            }
        }

        [System.NonSerialized]
        private Dictionary<Currency, int> _cacheRewardCurrencies;
        public Dictionary<Currency, int> CacheRewardCurrencies
        {
            get
            {
                if (_cacheRewardCurrencies == null)
                    _cacheRewardCurrencies = GameDataHelpers.CombineCurrencies(rewardCurrencies, null);
                return _cacheRewardCurrencies;
            }
        }

        public override bool Validate()
        {
            bool hasChanges = false;
            for (int i = 0; i < rewardCurrencies.Length; ++i)
            {
                if (rewardCurrencies[i].amount <= 0)
                {
                    Debug.LogWarning("[Quest] Reward Currencies [" + i + "], amount is " + rewardCurrencies[i].amount + " will be changed to 1 (Minimum Value)");
                    hasChanges = true;
                    CurrencyAmount reward = rewardCurrencies[i];
                    reward.amount = 1;
                    rewardCurrencies[i] = reward;
                }
            }
            for (int i = 0; i < rewardItems.Length; ++i)
            {
                if (rewardItems[i].amount <= 0)
                {
                    Debug.LogWarning("[Quest] Reward Items [" + i + "], amount is " + rewardItems[i].amount + " will be changed to 1 (Minimum Value)");
                    hasChanges = true;
                    ItemAmount reward = rewardItems[i];
                    reward.amount = 1;
                    rewardItems[i] = reward;
                }
            }
            for (int i = 0; i < selectableRewardItems.Length; ++i)
            {
                if (selectableRewardItems[i].amount <= 0)
                {
                    Debug.LogWarning("[Quest] Selectable Reward Items [" + i + "], amount is " + selectableRewardItems[i].amount + " will be changed to 1 (Minimum Value)");
                    hasChanges = true;
                    ItemAmount reward = selectableRewardItems[i];
                    reward.amount = 1;
                    selectableRewardItems[i] = reward;
                }
            }
            for (int i = 0; i < randomRewardItems.Length; ++i)
            {
                if (randomRewardItems[i].maxAmount <= 0)
                {
                    Debug.LogWarning("[Quest] Random Reward Items [" + i + "], max amount is " + randomRewardItems[i].maxAmount + " will be changed to 1 (Minimum Value)");
                    hasChanges = true;
                    ItemRandomByWeight reward = randomRewardItems[i];
                    reward.maxAmount = 1;
                    randomRewardItems[i] = reward;
                }
            }
            return hasChanges || base.Validate();
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            if (tasks != null && tasks.Length > 0)
            {
                foreach (QuestTask task in tasks)
                {
                    GameInstance.AddCharacters(task.monsterCharacterAmount.monster);
                    GameInstance.AddItems(task.itemAmount.item);
                    GameInstance.AddNpcDialogs(task.talkToNpcDialog);
                }
            }
            GameInstance.AddQuests(abandonQuests);
            GameInstance.AddCharacters(changeCharacterClass);
            GameInstance.AddFactions(changeCharacterFaction);
            GameInstance.AddCurrencies(rewardCurrencies);
            GameInstance.AddItems(rewardItems);
            GameInstance.AddItems(selectableRewardItems);
            GameInstance.AddItems(randomRewardItems);
            GameInstance.AddQuests(requirement.completedQuests);
        }

        public bool HaveToTalkToNpc(IPlayerCharacterData character, NpcEntity npcEntity, out int taskIndex, out BaseNpcDialog dialog, out bool completeAfterTalked)
        {
            taskIndex = -1;
            dialog = null;
            completeAfterTalked = false;
            if (tasks == null || tasks.Length == 0)
                return false;
            int indexOfQuest = character.IndexOfQuest(DataId);
            if (indexOfQuest < 0 || character.Quests[indexOfQuest].isComplete)
                return false;
            for (int i = 0; i < tasks.Length; ++i)
            {
                if (tasks[i].taskType != QuestTaskType.TalkToNpc ||
                    tasks[i].npcEntity == null)
                    continue;
                if (tasks[i].npcEntity.EntityId == npcEntity.EntityId)
                {
                    taskIndex = i;
                    dialog = tasks[i].talkToNpcDialog;
                    completeAfterTalked = tasks[i].completeAfterTalked;
                    return true;
                }
            }
            return false;
        }

        public bool CanReceiveQuest(IPlayerCharacterData character)
        {
            // Quest is completed, so don't show the menu which navigate to this dialog
            int indexOfQuest = character.IndexOfQuest(DataId);
            if (indexOfQuest >= 0 && character.Quests[indexOfQuest].isComplete)
                return false;
            // Character's level is lower than requirement
            if (character.Level < requirement.level)
                return false;
            // Character's has difference class
            if (requirement.character != null && requirement.character.DataId != character.DataId)
                return false;
            // Character's not complete all required quests
            if (requirement.completedQuests != null && requirement.completedQuests.Length > 0)
            {
                foreach (Quest quest in requirement.completedQuests)
                {
                    indexOfQuest = character.IndexOfQuest(quest.DataId);
                    if (indexOfQuest < 0)
                        return false;
                    if (!character.Quests[indexOfQuest].isComplete)
                        return false;
                }
            }
            return true;
        }
    }

    [System.Serializable]
    public struct QuestTask
    {
        public QuestTaskType taskType;
        [StringShowConditional(nameof(taskType), nameof(QuestTaskType.KillMonster))]
        public MonsterCharacterAmount monsterCharacterAmount;
        [StringShowConditional(nameof(taskType), nameof(QuestTaskType.CollectItem))]
        public ItemAmount itemAmount;
        [StringShowConditional(nameof(taskType), nameof(QuestTaskType.CollectItem))]
        [Tooltip("If this is `TRUE`, it will not decrease task items when quest completed")]
        public bool doNotDecreaseItemsOnQuestComplete;
        [StringShowConditional(nameof(taskType), nameof(QuestTaskType.TalkToNpc))]
        [Tooltip("Have to talk to this NPC to complete task")]
        public NpcEntity npcEntity;
        [StringShowConditional(nameof(taskType), nameof(QuestTaskType.TalkToNpc))]
        [Tooltip("This dialog will be shown immediately instead of start dialog which set to the NPC")]
        public BaseNpcDialog talkToNpcDialog;
        [StringShowConditional(nameof(taskType), nameof(QuestTaskType.TalkToNpc))]
        [Tooltip("If this is `TRUE` quest will be completed immediately after talked to NPC and all tasks done")]
        public bool completeAfterTalked;
        [StringShowConditional(nameof(taskType), nameof(QuestTaskType.Custom))]
        public BaseCustomQuestTask customQuestTask;
    }
}
