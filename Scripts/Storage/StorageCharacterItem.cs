using LiteNetLib.Utils;
using LiteNetLibManager;

public enum StorageType
{
    Default,
    Building,
}

[System.Serializable]
public sealed class StorageCharacterItem : CharacterItem
{
    public StorageType storageType;
    public int storageDataId;
    // Owner Id, for `Default` it is character Id. `Building` it is building Id
    public string storageOwnerId;
    public int storageIndex;

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put((byte)storageType);
        writer.Put(storageDataId);
        writer.Put(storageOwnerId);
        writer.Put(storageIndex);
        base.Serialize(writer);
    }

    public override void Deserialize(NetDataReader reader)
    {
        storageType = (StorageType)reader.GetByte();
        storageDataId = reader.GetInt();
        storageOwnerId = reader.GetString();
        storageIndex = reader.GetInt();
        base.Deserialize(reader);
    }
}

[System.Serializable]
public class SyncListStorageCharacterItem : LiteNetLibSyncList<StorageCharacterItem>
{
}
