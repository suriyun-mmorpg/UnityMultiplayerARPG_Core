using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

[System.Serializable]
public class CharacterQuest : INetSerializable
{
    public static readonly CharacterQuest Empty = new CharacterQuest();
    public int dataId;
    public bool isComplete;
    public Dictionary<int, int> killedMonsters = new Dictionary<int, int>();
    public List<int> completedTasks = new List<int>();

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

    public bool IsAllTasksDone(ICharacterData character)
    {
        Quest quest = GetQuest();
        if (character == null || quest == null)
            return false;
        QuestTask[] tasks = quest.tasks;
        for (int i = 0; i < tasks.Length; ++i)
        {
            bool isComplete = false;
            GetProgress(character, i, out isComplete);
            if (!isComplete)
                return false;
        }
        return true;
    }

    public int GetProgress(ICharacterData character, int taskIndex, out bool isComplete)
    {
        isComplete = false;
        Quest quest = GetQuest();
        if (character == null || quest == null || taskIndex < 0 || taskIndex >= quest.tasks.Length)
            return 0;
        QuestTask task = quest.tasks[taskIndex];
        int progress = 0;
        switch (task.taskType)
        {
            case QuestTaskType.KillMonster:
                MonsterCharacterAmount monsterCharacterAmount = task.monsterCharacterAmount;
                progress = monsterCharacterAmount.monster == null ? 0 : CountKillMonster(monsterCharacterAmount.monster.DataId);
                isComplete = progress >= monsterCharacterAmount.amount;
                return progress;
            case QuestTaskType.CollectItem:
                ItemAmount itemAmount = task.itemAmount;
                progress = itemAmount.item == null ? 0 : character.CountNonEquipItems(itemAmount.item.DataId);
                isComplete = progress >= itemAmount.amount;
                return progress;
        }
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

    public static CharacterQuest Create(Quest quest)
    {
        CharacterQuest newQuest = new CharacterQuest();
        newQuest.dataId = quest.DataId;
        newQuest.isComplete = false;
        return newQuest;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.PutPackedInt(dataId);
        writer.Put(isComplete);
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
        byte talkedNpcsCount = (byte)CompletedTasks.Count;
        writer.Put(talkedNpcsCount);
        if (talkedNpcsCount > 0)
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
        int killMonstersCount = reader.GetByte();
        KilledMonsters.Clear();
        for (int i = 0; i < killMonstersCount; ++i)
        {
            KilledMonsters.Add(reader.GetPackedInt(), reader.GetPackedInt());
        }
        int talkedNpcsCount = reader.GetByte();
        CompletedTasks.Clear();
        for (int i = 0; i < talkedNpcsCount; ++i)
        {
            CompletedTasks.Add(reader.GetPackedInt());
        }
    }
}

[System.Serializable]
public class SyncListCharacterQuest : LiteNetLibSyncList<CharacterQuest>
{
}
