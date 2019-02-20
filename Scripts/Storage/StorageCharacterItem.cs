using LiteNetLib.Utils;
using LiteNetLibManager;

public enum StorageType
{
    Default,
    Guild,
    Building,
}

[System.Serializable]
public sealed class StorageCharacterItem : INetSerializable
{
    public static readonly StorageCharacterItem Empty = new StorageCharacterItem();
    public StorageType storageType;
    public int storageDataId;
    // Owner Id, for `Default` it is character Id. `Building` it is building Id
    public string storageOwnerId;
    public CharacterItem characterItem;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((byte)storageType);
        writer.Put(storageDataId);
        writer.Put(storageOwnerId);
        characterItem.Serialize(writer);
    }

    public void Deserialize(NetDataReader reader)
    {
        storageType = (StorageType)reader.GetByte();
        storageDataId = reader.GetInt();
        storageOwnerId = reader.GetString();
        CharacterItem tempCharacterItem = new CharacterItem();
        tempCharacterItem.Deserialize(reader);
        characterItem = tempCharacterItem;
    }
}

public struct StorageId
{
    public StorageType storageType;
    public int storageDataId;
    public string storageOwnerId;

    public StorageId(StorageType storageType, int storageDataId, string storageOwnerId)
    {
        this.storageType = storageType;
        this.storageDataId = storageDataId;
        this.storageOwnerId = storageOwnerId;
    }

    public string GetId()
    {
        return (byte)storageType + "_" + storageDataId + "_" + storageOwnerId;
    }
}

public struct StorageItemId
{
    public StorageType storageType;
    public int storageDataId;
    public string storageOwnerId;
    public int indexOfData;

    public StorageItemId(StorageType storageType, int storageDataId, string storageOwnerId, int indexOfData)
    {
        this.storageType = storageType;
        this.storageDataId = storageDataId;
        this.storageOwnerId = storageOwnerId;
        this.indexOfData = indexOfData;
    }

    public string GetId()
    {
        return (byte)storageType + "_" + storageDataId + "_" + storageOwnerId + "_" + indexOfData;
    }
}
