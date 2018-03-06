using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public class CharacterAttributeLevel
{
    public string attributeId;
    public int level;

    public CharacterAttribute GetAttribute()
    {
        return GameInstance.CharacterAttributes.ContainsKey(attributeId) ? GameInstance.CharacterAttributes[attributeId] : null;
    }

    public bool CanLevelUp()
    {
        return GetAttribute() != null;
    }

    public CharacterStats GetStats()
    {
        var attribute = GetAttribute();
        if (attribute == null)
            return new CharacterStats();
        return attribute.statsIncreaseEachLevel * level;
    }

    public CharacterStatsPercentage GetStatsPercentage()
    {
        var attribute = GetAttribute();
        if (attribute == null)
            return new CharacterStatsPercentage();
        return attribute.statsPercentageIncreaseEachLevel * level;
    }
}

public class NetFieldCharacterAttributeLevel : LiteNetLibNetField<CharacterAttributeLevel>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterAttributeLevel();
        newValue.attributeId = reader.GetString();
        newValue.level = reader.GetInt();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        if (Value == null)
            Value = new CharacterAttributeLevel();
        writer.Put(Value.attributeId);
        writer.Put(Value.level);
    }

    public override bool IsValueChanged(CharacterAttributeLevel newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterAttributeLevel : LiteNetLibSyncList<NetFieldCharacterAttributeLevel, CharacterAttributeLevel> { }
