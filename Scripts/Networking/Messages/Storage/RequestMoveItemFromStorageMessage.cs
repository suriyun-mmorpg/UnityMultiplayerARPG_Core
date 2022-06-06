using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestMoveItemFromStorageMessage : INetSerializable
    {
        public StorageType storageType;
        public string storageOwnerId;
        public short storageItemIndex;
        public short storageItemAmount;
        public short inventoryItemIndex;
        public InventoryType inventoryType;
        public byte equipSlotIndexOrWeaponSet;

        public void Deserialize(NetDataReader reader)
        {
            storageType = (StorageType)reader.GetByte();
            storageOwnerId = reader.GetString();
            storageItemIndex = reader.GetPackedShort();
            storageItemAmount = reader.GetPackedShort();
            inventoryItemIndex = reader.GetPackedShort();
            inventoryType = (InventoryType)reader.GetByte();
            equipSlotIndexOrWeaponSet = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)storageType);
            writer.Put(storageOwnerId);
            writer.PutPackedShort(storageItemIndex);
            writer.PutPackedShort(storageItemAmount);
            writer.PutPackedShort(inventoryItemIndex);
            writer.Put((byte)inventoryType);
            writer.Put(equipSlotIndexOrWeaponSet);
        }
    }
}
