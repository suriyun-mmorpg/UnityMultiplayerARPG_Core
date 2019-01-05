using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

[System.Serializable]
public class CharacterSkill : INetSerializable
{
    public static readonly CharacterSkill Empty = new CharacterSkill();
    public int dataId;
    public short level;
    [System.NonSerialized]
    private int dirtyDataId;
    [System.NonSerialized]
    private Skill cacheSkill;

    private void MakeCache()
    {
        if (dirtyDataId != dataId)
        {
            dirtyDataId = dataId;
            cacheSkill = null;
            GameInstance.Skills.TryGetValue(dataId, out cacheSkill);
        }
    }

    public Skill GetSkill()
    {
        MakeCache();
        return cacheSkill;
    }

    public bool CanLevelUp(IPlayerCharacterData character)
    {
        return GetSkill().CanLevelUp(character, level);
    }

    public bool CanUse(ICharacterData character)
    {
        return GetSkill().CanUse(character, level);
    }

    public static CharacterSkill Create(Skill skill, short level)
    {
        CharacterSkill newSkill = new CharacterSkill();
        newSkill.dataId = skill.DataId;
        newSkill.level = level;
        return newSkill;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(dataId);
        writer.Put(level);
    }

    public void Deserialize(NetDataReader reader)
    {
        dataId = reader.GetInt();
        level = reader.GetShort();
    }
}

[System.Serializable]
public class SyncListCharacterSkill : LiteNetLibSyncList<CharacterSkill>
{
}
