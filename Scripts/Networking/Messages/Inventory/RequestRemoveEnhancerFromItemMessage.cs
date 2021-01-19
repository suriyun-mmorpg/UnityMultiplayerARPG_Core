using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestRemoveEnhancerFromItemMessage : INetSerializable
    {
        public InventoryType inventoryType;
        public short index;
        public short socketIndex;

        public void Deserialize(NetDataReader reader)
        {
            inventoryType = (InventoryType)reader.GetByte();
            index = reader.GetPackedShort();
            socketIndex = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)inventoryType);
            writer.PutPackedShort(index);
            writer.PutPackedShort(socketIndex);
        }
    }
}
