using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

[System.Serializable]
public class CharacterAttribute : INetSerializableWithElement
{
    public static readonly CharacterAttribute Empty = new CharacterAttribute();
    public int dataId;
    public short amount;

    [System.NonSerialized]
    private int dirtyDataId;

    [System.NonSerialized]
    private Attribute cacheAttribute;

    [System.NonSerialized]
    private LiteNetLibElement element;
    public LiteNetLibElement Element
    {
        get { return element; }
        set { element = value; }
    }

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

    public static CharacterAttribute Create(Attribute attribute, short amount)
    {
        CharacterAttribute newAttribute = new CharacterAttribute();
        newAttribute.dataId = attribute.DataId;
        newAttribute.amount = amount;
        return newAttribute;
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
