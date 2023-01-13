using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestEnhanceSocketItemMessage : INetSerializable
    {
        public InventoryType inventoryType;
        public int index;
        public int enhancerId;
        public int socketIndex;

        public void Deserialize(NetDataReader reader)
        {
            inventoryType = (InventoryType)reader.GetByte();
            index = reader.GetPackedInt();
            enhancerId = reader.GetPackedInt();
            socketIndex = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)inventoryType);
            writer.PutPackedInt(index);
            writer.PutPackedInt(enhancerId);
            writer.PutPackedInt(socketIndex);
        }
    }
}
