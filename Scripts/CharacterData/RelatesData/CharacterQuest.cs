using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;

[System.Serializable]
public class CharacterQuest
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
        var quest = GetQuest();
        if (character == null || quest == null)
            return false;
        var tasks = quest.tasks;
        for (var i = 0; i < tasks.Length; ++i)
        {
            var isComplete = false;
            GetProgress(character, i, out isComplete);
            if (!isComplete)
                return false;
        }
        return true;
    }

    public int GetProgress(ICharacterData character, int taskIndex, out bool isComplete)
    {
        isComplete = false;
        var quest = GetQuest();
        if (character == null || quest == null || taskIndex < 0 || taskIndex >= quest.tasks.Length)
            return 0;
        var task = quest.tasks[taskIndex];
        var progress = 0;
        switch (task.taskType)
        {
            case QuestTaskType.KillMonster:
                var monsterCharacterAmount = task.monsterCharacterAmount;
                progress = monsterCharacterAmount.monster == null ? 0 : CountKillMonster(monsterCharacterAmount.monster.DataId);
                isComplete = progress >= monsterCharacterAmount.amount;
                return progress;
            case QuestTaskType.CollectItem:
                var itemAmount = task.itemAmount;
                progress = itemAmount.item == null ? 0 : character.CountNonEquipItems(itemAmount.item.DataId);
                isComplete = progress >= itemAmount.amount;
                return progress;
        }
        return 0;
    }

    public bool AddKillMonster(MonsterCharacterEntity monsterEntity, int killCount)
    {
        return AddKillMonster(monsterEntity.DataId, killCount);
    }

    public bool AddKillMonster(int monsterDataId, int killCount)
    {
        var quest = GetQuest();
        if (quest == null || !quest.CacheKillMonsterIds.Contains(monsterDataId))
            return false;
        if (!KilledMonsters.ContainsKey(monsterDataId))
            KilledMonsters.Add(monsterDataId, 0);
        KilledMonsters[monsterDataId] += killCount;
        return true;
    }

    public int CountKillMonster(int monsterDataId)
    {
        var count = 0;
        KilledMonsters.TryGetValue(monsterDataId, out count);
        return count;
    }

    public static CharacterQuest Create(Quest quest)
    {
        var newQuest = new CharacterQuest();
        newQuest.dataId = quest.DataId;
        newQuest.isComplete = false;
        return newQuest;
    }
}

public class NetFieldCharacterQuest : LiteNetLibNetField<CharacterQuest>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterQuest();
        newValue.dataId = reader.GetInt();
        newValue.isComplete = reader.GetBool();
        var killMonsterCount = reader.GetInt();
        for (var i = 0; i < killMonsterCount; ++i)
        {
            newValue.KilledMonsters.Add(reader.GetInt(), reader.GetInt());
        }
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.dataId);
        writer.Put(Value.isComplete);
        var killedMonsters = Value.KilledMonsters;
        var killMonsterCount = killedMonsters.Count;
        writer.Put(killMonsterCount);
        if (killMonsterCount > 0)
        {
            foreach (var killedMonster in killedMonsters)
            {
                writer.Put(killedMonster.Key);
                writer.Put(killedMonster.Value);
            }
        }
    }

    public override bool IsValueChanged(CharacterQuest newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterQuest : LiteNetLibSyncList<NetFieldCharacterQuest, CharacterQuest>
{
}
