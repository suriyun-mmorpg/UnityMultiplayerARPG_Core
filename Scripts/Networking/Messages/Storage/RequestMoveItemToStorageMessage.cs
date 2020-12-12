using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestMoveItemToStorageMessage : INetSerializable
    {
        public string characterId;
        public StorageType storageType;
        public string storageOwnerId;
        public short inventoryIndex;
        public short amount;
        public short storageItemIndex;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            storageType = (StorageType)reader.GetByte();
            storageOwnerId = reader.GetString();
            inventoryIndex = reader.GetPackedShort();
            amount = reader.GetPackedShort();
            storageItemIndex = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.Put((byte)storageType);
            writer.Put(storageOwnerId);
            writer.PutPackedShort(inventoryIndex);
            writer.PutPackedShort(amount);
            writer.PutPackedShort(storageItemIndex);
        }
    }
}
