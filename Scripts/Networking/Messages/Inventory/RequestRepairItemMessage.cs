using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestRepairItemMessage : INetSerializable
    {
        public InventoryType inventoryType;
        public int index;

        public void Deserialize(NetDataReader reader)
        {
            inventoryType = (InventoryType)reader.GetByte();
            index = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)inventoryType);
            writer.PutPackedInt(index);
        }
    }
}
