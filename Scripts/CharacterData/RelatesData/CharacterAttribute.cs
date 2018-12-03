using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

[System.Serializable]
public class CharacterAttribute : INetSerializable
{
    public static readonly CharacterAttribute Empty = new CharacterAttribute();
    public int dataId;
    public short amount;
    [System.NonSerialized]
    private int dirtyDataId;
    [System.NonSerialized]
    private Attribute cacheAttribute;

    private void MakeCache()
    {
        if (dirtyDataId != dataId)
        {
            dirtyDataId = dataId;
            cacheAttribute = null;
            GameInstance.Attributes.TryGetValue(dataId, out cacheAttribute);
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

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(dataId);
        writer.Put(amount);
    }

    public void Deserialize(NetDataReader reader)
    {
        dataId = reader.GetInt();
        amount = reader.GetShort();
    }
}

[System.Serializable]
public class SyncListCharacterAttribute : LiteNetLibSyncList<CharacterAttribute>
{
}
