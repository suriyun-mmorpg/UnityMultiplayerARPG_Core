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

    [CreateAssetMenu(fileName = "Quest", menuName = "Create GameData/Quest")]
    public partial class Quest : BaseGameData
    {
        public QuestTask[] tasks;
        public int rewardExp;
        public int rewardGold;
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
