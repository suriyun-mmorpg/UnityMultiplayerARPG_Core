using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestDismantleItemMessage : INetSerializable
    {
        public int index;
        public int amount;

        public void Deserialize(NetDataReader reader)
        {
            index = reader.GetPackedInt();
            amount = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(index);
            writer.PutPackedInt(amount);
        }
    }
}
