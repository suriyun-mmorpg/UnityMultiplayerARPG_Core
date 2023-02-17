using System.Collections.Generic;
using Cysharp.Text;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class CharacterQuest : INetSerializable
    {
        [System.NonSerialized]
        private int dirtyDataId;

        [System.NonSerialized]
        private Quest cacheQuest;

        public Dictionary<int, int> KilledMonsters
        {
            get
            {
                if (killedMonsters == null)
                    killedMonsters = new Dictionary<int, int>();
                return killedMonsters;
            }
        }

        public List<int> CompletedTasks
        {
            get
            {
                if (completedTasks == null)
                    completedTasks = new List<int>();
                return completedTasks;
            }
        }

        private void MakeCache()
        {
            if (!GameInstance.Quests.ContainsKey(dataId))
            {
                cacheQuest = null;
                return;
            }
            if (dirtyDataId != dataId)
            {
                dirtyDataId = dataId;
                cacheQuest = GameInstance.Quests.TryGetValue(dataId, out cacheQuest) ? cacheQuest : null;
            }
        }

        public Quest GetQuest()
        {
            MakeCache();
            return cacheQuest;
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

        public Dictionary<int, int> ReadKilledMonsters(string killMonsters)
        {
            KilledMonsters.Clear();
            string[] splitSets = killMonsters.Split(';');
            foreach (string set in splitSets)
            {
                if (string.IsNullOrEmpty(set))
                    continue;
                string[] splitData = set.Split(':');
                if (splitData.Length != 2)
                    continue;
                KilledMonsters[int.Parse(splitData[0])] = int.Parse(splitData[1]);
            }
            return KilledMonsters;
        }

        public string WriteKilledMonsters()
        {
            using (Utf16ValueStringBuilder stringBuilder = ZString.CreateStringBuilder(true))
            {
                foreach (KeyValuePair<int, int> keyValue in KilledMonsters)
                {
                    stringBuilder.Append(keyValue.Key);
                    stringBuilder.Append(':');
                    stringBuilder.Append(keyValue.Value);
                    stringBuilder.Append(';');
                }
                return stringBuilder.ToString();
            }
        }

        public List<int> ReadCompletedTasks(string completedTasks)
        {
            CompletedTasks.Clear();
            string[] splitTexts = completedTasks.Split(';');
            foreach (string text in splitTexts)
            {
                if (string.IsNullOrEmpty(text))
                    continue;
                CompletedTasks.Add(int.Parse(text));
            }
            return CompletedTasks;
        }

        public string WriteCompletedTasks()
        {
            using (Utf16ValueStringBuilder stringBuilder = ZString.CreateStringBuilder(true))
            {
                foreach (int completedTask in CompletedTasks)
                {
                    stringBuilder.Append(completedTask);
                    stringBuilder.Append(';');
                }
                return stringBuilder.ToString();
            }
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
