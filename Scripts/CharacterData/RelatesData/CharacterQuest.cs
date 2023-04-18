using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class CharacterQuest : INetSerializable
    {
        [System.NonSerialized]
        private int _dirtyDataId;

        [System.NonSerialized]
        private Quest _cacheQuest;

        private void MakeCache()
        {
            if (_dirtyDataId == dataId)
                return;
            _dirtyDataId = dataId;
            if (!GameInstance.Quests.TryGetValue(dataId, out _cacheQuest))
                _cacheQuest = null;
        }

        public Quest GetQuest()
        {
            MakeCache();
            return _cacheQuest;
        }

        public bool IsAllTasksDone(IPlayerCharacterData character)
        {
            Quest quest = GetQuest();
            if (character == null || quest == null)
                return false;
            QuestTask[] tasks = quest.tasks;
            for (int i = 0; i < tasks.Length; ++i)
            {
                bool isComplete;
                GetProgress(character, i, out isComplete);
                if (!isComplete)
                    return false;
            }
            return true;
        }

        public bool IsAllTasksDoneAndIsCompletingTarget(IPlayerCharacterData character, NpcEntity npcEntity)
        {
            Quest quest = GetQuest();
            if (character == null || quest == null)
                return false;
            QuestTask[] tasks = quest.tasks;
            for (int i = 0; i < tasks.Length; ++i)
            {
                bool isComplete;
                GetProgress(character, i, out isComplete);
                if (!isComplete)
                    return false;
                if (tasks[i].taskType == QuestTaskType.TalkToNpc && tasks[i].completeAfterTalked &&
                    (npcEntity == null || tasks[i].npcEntity.EntityId != npcEntity.EntityId))
                    return false;
            }
            return true;
        }

        public int GetProgress(IPlayerCharacterData character, int taskIndex, out bool isComplete)
        {
            return GetProgress(character, taskIndex, out _, out _, out isComplete);
        }

        public int GetProgress(IPlayerCharacterData character, int taskIndex, out string targetTitle, out int targetProgress, out bool isComplete)
        {
            Quest quest = GetQuest();
            if (character == null || quest == null || taskIndex < 0 || taskIndex >= quest.tasks.Length)
            {
                targetTitle = string.Empty;
                targetProgress = 0;
                isComplete = false;
                return 0;
            }
            QuestTask task = quest.tasks[taskIndex];
            int progress;
            switch (task.taskType)
            {
                case QuestTaskType.KillMonster:
                    targetTitle = task.monsterCharacterAmount.monster == null ? string.Empty : task.monsterCharacterAmount.monster.Title;
                    progress = task.monsterCharacterAmount.monster == null ? 0 : CountKillMonster(task.monsterCharacterAmount.monster.DataId);
                    targetProgress = task.monsterCharacterAmount.amount;
                    isComplete = progress >= targetProgress;
                    return progress;
                case QuestTaskType.CollectItem:
                    targetTitle = task.itemAmount.item == null ? string.Empty : task.itemAmount.item.Title;
                    progress = task.itemAmount.item == null ? 0 : character.CountNonEquipItems(task.itemAmount.item.DataId);
                    targetProgress = task.itemAmount.amount;
                    isComplete = progress >= targetProgress;
                    return progress;
                case QuestTaskType.TalkToNpc:
                    targetTitle = task.npcEntity == null ? null : task.npcEntity.Title;
                    if (task.completeAfterTalked)
                        progress = 1;
                    else
                        progress = CompletedTasks.Contains(taskIndex) ? 1 : 0;
                    targetProgress = 1;
                    isComplete = progress >= targetProgress;
                    return progress;
                case QuestTaskType.Custom:
                    return task.customQuestTask.GetTaskProgress(character, quest, taskIndex, out targetTitle, out targetProgress, out isComplete);
            }
            targetTitle = string.Empty;
            targetProgress = 0;
            isComplete = false;
            return 0;
        }

        public bool AddKillMonster(BaseMonsterCharacterEntity monsterEntity, int killCount)
        {
            return AddKillMonster(monsterEntity.DataId, killCount);
        }

        public bool AddKillMonster(int monsterDataId, int killCount)
        {
            Quest quest = GetQuest();
            if (quest == null || !quest.CacheKillMonsterIds.Contains(monsterDataId))
                return false;
            if (!KilledMonsters.ContainsKey(monsterDataId))
                KilledMonsters.Add(monsterDataId, 0);
            KilledMonsters[monsterDataId] += killCount;
            return true;
        }

        public int CountKillMonster(int monsterDataId)
        {
            if (!KilledMonsters.ContainsKey(monsterDataId))
                return 0;
            return KilledMonsters[monsterDataId];
        }

        public CharacterQuest Clone()
        {
            CharacterQuest clone = new CharacterQuest();
            clone.dataId = dataId;
            clone.isComplete = isComplete;
            clone.isTracking = isTracking;
            // Clone killed monsters
            Dictionary<int, int> killedMonsters = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> cloneEntry in this.killedMonsters)
            {
                killedMonsters[cloneEntry.Key] = cloneEntry.Value;
            }
            clone.killedMonsters = killedMonsters;
            // Clone complete tasks
            clone.completedTasks = new List<int>(completedTasks);
            return clone;
        }

        public static CharacterQuest Create(Quest quest)
        {
            return new CharacterQuest()
            {
                dataId = quest.DataId,
                isComplete = false,
            };
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(dataId);
            writer.Put(isComplete);
            writer.Put(isTracking);
            byte killMonstersCount = (byte)KilledMonsters.Count;
            writer.Put(killMonstersCount);
            if (killMonstersCount > 0)
            {
                foreach (KeyValuePair<int, int> killedMonster in KilledMonsters)
                {
                    writer.PutPackedInt(killedMonster.Key);
                    writer.PutPackedInt(killedMonster.Value);
                }
            }
            byte completedTasksCount = (byte)CompletedTasks.Count;
            writer.Put(completedTasksCount);
            if (completedTasksCount > 0)
            {
                foreach (int talkedNpc in CompletedTasks)
                {
                    writer.PutPackedInt(talkedNpc);
                }
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            dataId = reader.GetPackedInt();
            isComplete = reader.GetBool();
            isTracking = reader.GetBool();
            int killMonstersCount = reader.GetByte();
            KilledMonsters.Clear();
            for (int i = 0; i < killMonstersCount; ++i)
            {
                KilledMonsters.Add(reader.GetPackedInt(), reader.GetPackedInt());
            }
            int completedTasksCount = reader.GetByte();
            CompletedTasks.Clear();
            for (int i = 0; i < completedTasksCount; ++i)
            {
                CompletedTasks.Add(reader.GetPackedInt());
            }
        }
    }

    [System.Serializable]
    public class SyncListCharacterQuest : LiteNetLibSyncList<CharacterQuest>
    {
    }
}
