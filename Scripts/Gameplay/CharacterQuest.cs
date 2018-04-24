using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public struct CharacterQuest
{
    public static readonly CharacterQuest Empty = new CharacterQuest();
    public string questId;
    public bool isDone;
    public Dictionary<string, int> killedMonsters;
    [System.NonSerialized]
    private string dirtyQuestId;
    [System.NonSerialized]
    private Quest cacheQuest;

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

    public int GetProgress(ICharacterData character, int taskIndex)
    {
        var quest = GetQuest();
        if (character == null || quest == null || taskIndex < 0 || taskIndex >= quest.tasks.Length)
            return 0;
        var task = quest.tasks[taskIndex];
        switch (task.taskType)
        {
            case QuestTaskType.KillMonster:
                var monsterCharacterAmount = task.monsterCharacterAmount;
                return monsterCharacterAmount.monster == null ? 0 : CountKillMonster(monsterCharacterAmount.monster.Id);
            case QuestTaskType.CollectItem:
                var itemAmount = task.itemAmount;
                return itemAmount.item == null ? 0 : character.CountNonEquipItems(itemAmount.item.Id);
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
        if (killedMonsters == null)
            killedMonsters = new Dictionary<string, int>();
        if (!killedMonsters.ContainsKey(monsterId))
            killedMonsters.Add(monsterId, 0);
        killedMonsters[monsterId] += killCount;
    }

    public int CountKillMonster(string monsterId)
    {
        var count = 0;
        if (!string.IsNullOrEmpty(monsterId))
            killedMonsters.TryGetValue(monsterId, out count);
        return count;
    }
}

public class NetFieldCharacterQuest : LiteNetLibNetField<CharacterQuest>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterQuest();
        newValue.questId = reader.GetString();
        newValue.isDone = reader.GetBool();
        var killMonsterCount = reader.GetInt();
        newValue.killedMonsters = new Dictionary<string, int>();
        for (var i = 0; i < killMonsterCount; ++i)
        {
            newValue.killedMonsters.Add(reader.GetString(), reader.GetInt());
        }
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.questId);
        writer.Put(Value.isDone);
        var killedMonsters = Value.killedMonsters;
        var killMonsterCount = killedMonsters == null ? 0 : killedMonsters.Count;
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
