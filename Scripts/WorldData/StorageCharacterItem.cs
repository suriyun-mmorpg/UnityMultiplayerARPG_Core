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
    public string storageId;
    public int storageIndex;

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put((byte)storageType);
        writer.Put(storageId);
        writer.Put(storageIndex);
        base.Serialize(writer);
    }

    public override void Deserialize(NetDataReader reader)
    {
        storageType = (StorageType)reader.GetByte();
        storageId = reader.GetString();
        storageIndex = reader.GetInt();
        base.Deserialize(reader);
    }
}

[System.Serializable]
public class SyncListStorageCharacterItem : LiteNetLibSyncList<StorageCharacterItem>
{
}
