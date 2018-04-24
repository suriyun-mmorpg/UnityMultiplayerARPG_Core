using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public struct CharacterAttribute
{
    public static readonly CharacterAttribute Empty = new CharacterAttribute();
    public string attributeId;
    public int amount;
    [System.NonSerialized]
    private string dirtyAttributeId;
    [System.NonSerialized]
    private Attribute cacheAttribute;

    private void MakeCache()
    {
        if (string.IsNullOrEmpty(attributeId))
        {
            cacheAttribute = null;
            return;
        }
        if (string.IsNullOrEmpty(dirtyAttributeId) || !dirtyAttributeId.Equals(attributeId))
        {
            dirtyAttributeId = attributeId;
            cacheAttribute = GameInstance.Attributes.TryGetValue(attributeId, out cacheAttribute) ? cacheAttribute : null;
        }
    }

    public bool IsEmpty()
    {
        return Equals(Empty);
    }

    public Attribute GetAttribute()
    {
        MakeCache();
        return cacheAttribute;
    }

    public bool CanIncrease(IPlayerCharacterData character)
    {
        return GetAttribute() != null && character.StatPoint > 0;
    }

    public void Increase(int amount)
    {
        this.amount += amount;
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
        writer.Put(Value.attributeId);
        writer.Put(Value.amount);
    }

    public override bool IsValueChanged(CharacterAttribute newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterAttribute : LiteNetLibSyncList<NetFieldCharacterAttribute, CharacterAttribute>
{
}
