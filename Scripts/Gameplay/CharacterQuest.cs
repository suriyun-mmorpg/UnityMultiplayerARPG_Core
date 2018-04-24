using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public struct CharacterQuest
{
    public static readonly CharacterQuest Empty = new CharacterQuest();
    public string questId;
    public bool isComplete;
    public Dictionary<string, int> killedMonsters;
    [System.NonSerialized]
    private string dirtyQuestId;
    [System.NonSerialized]
    private Quest cacheQuest;

    public Dictionary<string, int> KilledMonsters
    {
        get
        {
            if (killedMonsters == null)
                killedMonsters = new Dictionary<string, int>();
            return killedMonsters;
        }
    }

    private void MakeCache()
    {
        if (string.IsNullOrEmpty(questId))
        {
            cacheQuest = null;
            return;
        }
        if (string.IsNullOrEmpty(dirtyQuestId) || !dirtyQuestId.Equals(questId))
        {
            dirtyQuestId = questId;
            cacheQuest = GameInstance.Quests.TryGetValue(questId, out cacheQuest) ? cacheQuest : null;
        }
    }

    public bool IsEmpty()
    {
        return Equals(Empty);
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
                progress = monsterCharacterAmount.monster == null ? 0 : CountKillMonster(monsterCharacterAmount.monster.Id);
                isComplete = progress >= monsterCharacterAmount.amount;
                return progress;
            case QuestTaskType.CollectItem:
                var itemAmount = task.itemAmount;
                progress = itemAmount.item == null ? 0 : character.CountNonEquipItems(itemAmount.item.Id);
                isComplete = progress >= itemAmount.amount;
                return progress;
        }
        return 0;
    }

    public void AddKillMonster(MonsterCharacterEntity monsterEntity, int killCount)
    {
        AddKillMonster(monsterEntity.DatabaseId, killCount);
    }

    public void AddKillMonster(string monsterId, int killCount)
    {
        var quest = GetQuest();
        if (quest == null || !quest.CacheKillMonsterIds.Contains(monsterId))
            return;
        if (!KilledMonsters.ContainsKey(monsterId))
            KilledMonsters.Add(monsterId, 0);
        KilledMonsters[monsterId] += killCount;
    }

    public int CountKillMonster(string monsterId)
    {
        var count = 0;
        if (!string.IsNullOrEmpty(monsterId))
            KilledMonsters.TryGetValue(monsterId, out count);
        return count;
    }

    public static CharacterQuest Create(Quest quest)
    {
        var newQuest = new CharacterQuest();
        newQuest.questId = quest.Id;
        newQuest.isComplete = false;
        return newQuest;
    }
}

public class NetFieldCharacterQuest : LiteNetLibNetField<CharacterQuest>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterQuest();
        newValue.questId = reader.GetString();
        newValue.isComplete = reader.GetBool();
        var killMonsterCount = reader.GetInt();
        for (var i = 0; i < killMonsterCount; ++i)
        {
            newValue.KilledMonsters.Add(reader.GetString(), reader.GetInt());
        }
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.questId);
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
    public int IndexOf(string questId)
    {
        CharacterQuest tempQuest;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempQuest = list[i];
            if (!string.IsNullOrEmpty(tempQuest.questId) &&
                tempQuest.questId.Equals(questId))
            {
                index = i;
                break;
            }
        }
        return index;
    }
}
