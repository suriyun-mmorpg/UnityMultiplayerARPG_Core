using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestRefineItemMessage : INetSerializable
    {
        public InventoryType inventoryType;
        public int index;
        public int[] enhancerDataIds;

        public void Deserialize(NetDataReader reader)
        {
            inventoryType = (InventoryType)reader.GetByte();
            index = reader.GetPackedInt();
            enhancerDataIds = reader.GetIntArray();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)inventoryType);
            writer.PutPackedInt(index);
            writer.PutArray(enhancerDataIds);
        }
    }
}
