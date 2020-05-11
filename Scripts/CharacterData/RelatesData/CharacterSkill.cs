using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

[System.Serializable]
public class CharacterSkill : INetSerializableWithElement
{
    public static readonly CharacterSkill Empty = new CharacterSkill();
    public int dataId;
    public short level;

    [System.NonSerialized]
    private int dirtyDataId;
    [System.NonSerialized]
    private short dirtyLevel;

    [System.NonSerialized]
    private BaseSkill cacheSkill;

    [System.NonSerialized]
    private LiteNetLibElement element;
    public LiteNetLibElement Element
    {
        get { return element; }
        set { element = value; }
    }

    private void MakeCache()
    {
        if (dirtyDataId != dataId || dirtyLevel != level)
        {
            dirtyDataId = dataId;
            dirtyLevel = level;
            cacheSkill = null;
            GameInstance.Skills.TryGetValue(dataId, out cacheSkill);
        }
    }

    public BaseSkill GetSkill()
    {
        MakeCache();
        return cacheSkill;
    }

    public static CharacterSkill Create(BaseSkill skill, short level)
    {
        CharacterSkill newSkill = new CharacterSkill();
        newSkill.dataId = skill.DataId;
        newSkill.level = level;
        return newSkill;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.PutPackedInt(dataId);
        writer.PutPackedShort(level);
    }

    public void Deserialize(NetDataReader reader)
    {
        dataId = reader.GetPackedInt();
        level = reader.GetPackedShort();
    }
}

[System.Serializable]
public class SyncListCharacterSkill : LiteNetLibSyncList<CharacterSkill>
{
}
