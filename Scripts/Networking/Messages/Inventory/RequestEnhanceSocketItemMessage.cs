using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestEnhanceSocketItemMessage : INetSerializable
    {
        public InventoryType inventoryType;
        public short index;
        public int enhancerId;
        public short socketIndex;

        public void Deserialize(NetDataReader reader)
        {
            inventoryType = (InventoryType)reader.GetByte();
            index = reader.GetPackedShort();
            enhancerId = reader.GetPackedInt();
            socketIndex = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)inventoryType);
            writer.PutPackedShort(index);
            writer.PutPackedInt(enhancerId);
            writer.PutPackedShort(socketIndex);
        }
    }
}
