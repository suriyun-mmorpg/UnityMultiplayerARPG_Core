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
        int count = 0;
        KilledMonsters.TryGetValue(monsterDataId, out count);
        return count;
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
        writer.Put(dataId);
        writer.Put(isComplete);
        byte killMonsterCount = (byte)KilledMonsters.Count;
        writer.Put(killMonsterCount);
        if (killMonsterCount > 0)
        {
            foreach (KeyValuePair<int, int> killedMonster in KilledMonsters)
            {
                writer.Put(killedMonster.Key);
                writer.Put(killedMonster.Value);
            }
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        dataId = reader.GetInt();
        isComplete = reader.GetBool();
        int killMonsterCount = reader.GetByte();
        KilledMonsters.Clear();
        for (int i = 0; i < killMonsterCount; ++i)
        {
            KilledMonsters.Add(reader.GetInt(), reader.GetInt());
        }
    }
}

[System.Serializable]
public class SyncListCharacterQuest : LiteNetLibSyncList<CharacterQuest>
{
}
