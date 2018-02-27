using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public struct CharacterAttributeLevel
{
    // Use attributeId as primary key
    public string attributeId;
    public int amount;
    
    public CharacterAttribute Attribute
    {
        get { return GameInstance.CharacterAttributes.ContainsKey(attributeId) ? GameInstance.CharacterAttributes[attributeId] : null; }
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
        writer.Put(Value.attributeId);
        writer.Put(Value.amount);
    }

    public override bool IsValueChanged(CharacterAttributeLevel newValue)
    {
        return !newValue.Equals(Value);
    }
}

[System.Serializable]
public class SyncListCharacterAttributeLevel : LiteNetLibSyncList<NetFieldCharacterAttributeLevel, CharacterAttributeLevel> { }
