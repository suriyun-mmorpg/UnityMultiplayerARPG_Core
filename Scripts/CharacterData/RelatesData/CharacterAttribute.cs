using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;

[System.Serializable]
public class CharacterAttribute
{
    public static readonly CharacterAttribute Empty = new CharacterAttribute();
    public int dataId;
    public int amount;
    [System.NonSerialized]
    private int dirtyDataId;
    [System.NonSerialized]
    private Attribute cacheAttribute;

    private void MakeCache()
    {
        if (!GameInstance.Attributes.ContainsKey(dataId))
        {
            cacheAttribute = null;
            return;
        }
        if (dirtyDataId != dataId)
        {
            dirtyDataId = dataId;
            cacheAttribute = GameInstance.Attributes.TryGetValue(dataId, out cacheAttribute) ? cacheAttribute : null;
        }
    }

    public Attribute GetAttribute()
    {
        MakeCache();
        return cacheAttribute;
    }

    public bool CanIncrease(IPlayerCharacterData character)
    {
        return GetAttribute() != null && character != null && character.StatPoint > 0;
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
        newValue.dataId = reader.GetInt();
        newValue.amount = reader.GetInt();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.dataId);
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
