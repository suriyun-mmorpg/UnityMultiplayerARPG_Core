using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public class CharacterAttribute
{
    public string attributeId;
    public int amount;

    public Attribute GetAttribute()
    {
        return GameInstance.CharacterAttributes.ContainsKey(attributeId) ? GameInstance.CharacterAttributes[attributeId] : null;
    }

    public bool CanLevelUp()
    {
        return GetAttribute() != null;
    }
}

public class NetFieldCharacterAttribute : LiteNetLibNetField<CharacterAttribute>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterAttribute();
        newValue.attributeId = reader.GetString();
        newValue.amount = reader.GetInt();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        if (Value == null)
            Value = new CharacterAttribute();
        writer.Put(Value.attributeId);
        writer.Put(Value.amount);
    }

    public override bool IsValueChanged(CharacterAttribute newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterAttribute : LiteNetLibSyncList<NetFieldCharacterAttribute, CharacterAttribute> { }
