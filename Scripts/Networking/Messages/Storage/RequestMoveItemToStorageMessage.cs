using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestMoveItemToStorageMessage : INetSerializable
    {
        public StorageType storageType;
        public string storageOwnerId;
        public short inventoryItemIndex;
        public short inventoryItemAmount;
        public short storageItemIndex;

        public void Deserialize(NetDataReader reader)
        {
            storageType = (StorageType)reader.GetByte();
            storageOwnerId = reader.GetString();
            inventoryItemIndex = reader.GetPackedShort();
            inventoryItemAmount = reader.GetPackedShort();
            storageItemIndex = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)storageType);
            writer.Put(storageOwnerId);
            writer.PutPackedShort(inventoryItemIndex);
            writer.PutPackedShort(inventoryItemAmount);
            writer.PutPackedShort(storageItemIndex);
        }
    }
}
