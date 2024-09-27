using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestChangeAmmoItemMessage : INetSerializable
    {
        public InventoryType inventoryType;
        public int index;
        public string ammoItemId;

        public void Deserialize(NetDataReader reader)
        {
            inventoryType = (InventoryType)reader.GetByte();
            index = reader.GetPackedInt();
            ammoItemId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)inventoryType);
            writer.PutPackedInt(index);
            writer.Put(ammoItemId);
        }
    }
}
