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
        public QuestTask[] tasks;
        public int rewardExp;
        public int rewardGold;
        [ArrayElementTitle("item", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
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
            List<MonsterCharacter> monsters = new List<MonsterCharacter>();
            List<Item> items = new List<Item>();
            if (tasks != null && tasks.Length > 0)
            {
                foreach (QuestTask task in tasks)
                {
                    if (task.monsterCharacterAmount.monster != null)
                        monsters.Add(task.monsterCharacterAmount.monster);
                    if (task.itemAmount.item != null)
                        items.Add(task.itemAmount.item);
                }
            }
            if (rewardItems != null && rewardItems.Length > 0)
            {
                foreach (ItemAmount rewardItem in rewardItems)
                {
                    if (rewardItem.item != null)
                        items.Add(rewardItem.item);
                }
            }
            GameInstance.AddCharacters(monsters);
            GameInstance.AddItems(items);
        }
    }

    [System.Serializable]
    public struct QuestTask
    {
        public QuestTaskType taskType;
        [StringShowConditional(conditionFieldName: "taskType", conditionValue: "KillMonster")]
        public MonsterCharacterAmount monsterCharacterAmount;
        [StringShowConditional(conditionFieldName: "taskType", conditionValue: "CollectItem")]
        public ItemAmount itemAmount;
    }
}
