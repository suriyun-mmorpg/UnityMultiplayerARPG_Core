using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum QuestTaskType : byte
    {
        KillMonster,
        CollectItem,
    }

    [CreateAssetMenu(fileName = "Quest", menuName = "Create GameData/Quest", order = -4796)]
    public partial class Quest : BaseGameData
    {
        [Header("Quest Configs")]
        [Tooltip("Requirement to receive quest")]
        public QuestRequirement requirement;
        public QuestTask[] tasks;
        public int rewardExp;
        public int rewardGold;
        [ArrayElementTitle("item")]
        public ItemAmount[] rewardItems;
        public bool canRepeat;
        private HashSet<int> cacheKillMonsterIds;
        public HashSet<int> CacheKillMonsterIds
        {
            get
            {
                if (cacheKillMonsterIds == null)
                {
                    cacheKillMonsterIds = new HashSet<int>();
                    foreach (QuestTask task in tasks)
                    {
                        if (task.taskType == QuestTaskType.KillMonster &&
                            task.monsterCharacterAmount.monster != null &&
                            task.monsterCharacterAmount.amount > 0)
                            cacheKillMonsterIds.Add(task.monsterCharacterAmount.monster.DataId);
                    }
                }
                return cacheKillMonsterIds;
            }
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
                }
            }
            if (rewardItems != null && rewardItems.Length > 0)
            {
                foreach (ItemAmount rewardItem in rewardItems)
                {
                    GameInstance.AddItems(rewardItem.item);
                }
            }
            GameInstance.AddQuests(requirement.completedQuests);
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
    }
}
