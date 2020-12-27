using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestSwapOrMergeStorageItemMessage : INetSerializable
    {
        public StorageType storageType;
        public string storageOwnerId;
        public short fromIndex;
        public short toIndex;

        public void Deserialize(NetDataReader reader)
        {
            storageType = (StorageType)reader.GetByte();
            storageOwnerId = reader.GetString();
            fromIndex = reader.GetPackedShort();
            toIndex = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)storageType);
            writer.Put(storageOwnerId);
            writer.PutPackedShort(fromIndex);
            writer.PutPackedShort(toIndex);
        }
    }
}
