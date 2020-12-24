using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestMoveItemFromStorageMessage : INetSerializable
    {
        public string characterId;
        public StorageType storageType;
        public string storageOwnerId;
        public short storageItemIndex;
        public short storageItemAmount;
        public short inventoryItemIndex;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            storageType = (StorageType)reader.GetByte();
            storageOwnerId = reader.GetString();
            storageItemIndex = reader.GetPackedShort();
            storageItemAmount = reader.GetPackedShort();
            inventoryItemIndex = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.Put((byte)storageType);
            writer.Put(storageOwnerId);
            writer.PutPackedShort(storageItemIndex);
            writer.PutPackedShort(storageItemAmount);
            writer.PutPackedShort(inventoryItemIndex);
        }
    }
}
