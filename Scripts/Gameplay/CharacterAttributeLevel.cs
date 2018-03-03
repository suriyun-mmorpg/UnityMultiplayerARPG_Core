using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public class CharacterAttributeLevel
{
    // Use attributeId as primary key
    public string attributeId;
    public int amount;

    public CharacterAttribute GetAttribute()
    {
        return GameInstance.CharacterAttributes.ContainsKey(attributeId) ? GameInstance.CharacterAttributes[attributeId] : null;
    }

    public CharacterStats GetStats()
    {
        var attribute = GetAttribute();
        if (attribute == null)
            return new CharacterStats();
        return attribute.statsIncreaseEachLevel * amount;
    }

    public CharacterStatsPercentage GetStatsPercentage()
    {
        var attribute = GetAttribute();
        if (attribute == null)
            return new CharacterStatsPercentage();
        return attribute.statsPercentageIncreaseEachLevel * amount;
    }
}

public class NetFieldCharacterAttributeLevel : LiteNetLibNetField<CharacterAttributeLevel>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterAttributeLevel();
        newValue.attributeId = reader.GetString();
        newValue.amount = reader.GetInt();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        if (Value == null)
            Value = new CharacterAttributeLevel();
        writer.Put(Value.attributeId);
        writer.Put(Value.amount);
    }

    public override bool IsValueChanged(CharacterAttributeLevel newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterAttributeLevel : LiteNetLibSyncList<NetFieldCharacterAttributeLevel, CharacterAttributeLevel> { }
