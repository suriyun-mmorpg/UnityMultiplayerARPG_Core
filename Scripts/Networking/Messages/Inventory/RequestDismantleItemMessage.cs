using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestDismantleItemMessage : INetSerializable
    {
        public short index;
        public short amount;

        public void Deserialize(NetDataReader reader)
        {
            index = reader.GetPackedShort();
            amount = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedShort(index);
            writer.PutPackedShort(amount);
        }
    }
}
