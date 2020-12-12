using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestMoveItemFromStorageMessage : INetSerializable
    {
        public string characterId;
        public StorageType storageType;
        public string storageOwnerId;
        public short storageItemIndex;
        public short amount;
        public short inventoryIndex;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            storageType = (StorageType)reader.GetByte();
            storageOwnerId = reader.GetString();
            storageItemIndex = reader.GetPackedShort();
            amount = reader.GetPackedShort();
            inventoryIndex = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.Put((byte)storageType);
            writer.Put(storageOwnerId);
            writer.PutPackedShort(storageItemIndex);
            writer.PutPackedShort(amount);
            writer.PutPackedShort(inventoryIndex);
        }
    }
}
