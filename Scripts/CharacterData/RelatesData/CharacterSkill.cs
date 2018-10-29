using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

[System.Serializable]
public class CharacterSkill
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

    public void LevelUp(short level)
    {
        this.level += level;
    }

    public bool CanUse(ICharacterData character)
    {
        return GetSkill().CanUse(character, level);
    }

    public static CharacterSkill Create(Skill skill, short level)
    {
        var newSkill = new CharacterSkill();
        newSkill.dataId = skill.DataId;
        newSkill.level = level;
        return newSkill;
    }
}

public class NetFieldCharacterSkill : LiteNetLibNetField<CharacterSkill>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterSkill();
        newValue.dataId = reader.GetInt();
        newValue.level = reader.GetShort();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.dataId);
        writer.Put(Value.level);
    }

    public override bool IsValueChanged(CharacterSkill newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterSkill : LiteNetLibSyncList<NetFieldCharacterSkill, CharacterSkill>
{
}
