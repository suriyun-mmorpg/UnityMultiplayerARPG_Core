using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestRepairItemMessage : INetSerializable
    {
        public InventoryType inventoryType;
        public short index;

        public void Deserialize(NetDataReader reader)
        {
            inventoryType = (InventoryType)reader.GetByte();
            index = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)inventoryType);
            writer.PutPackedShort(index);
        }
    }
}
