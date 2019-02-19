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

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put((byte)storageType);
        writer.Put(storageDataId);
        writer.Put(storageOwnerId);
        base.Serialize(writer);
    }

    public override void Deserialize(NetDataReader reader)
    {
        storageType = (StorageType)reader.GetByte();
        storageDataId = reader.GetInt();
        storageOwnerId = reader.GetString();
        base.Deserialize(reader);
    }
}

[System.Serializable]
public class SyncListStorageCharacterItem : LiteNetLibSyncList<StorageCharacterItem>
{
}
