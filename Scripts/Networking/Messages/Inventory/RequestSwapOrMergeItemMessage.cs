using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestSwapOrMergeItemMessage : INetSerializable
    {
        public short fromIndex;
        public short toIndex;

        public void Deserialize(NetDataReader reader)
        {
            fromIndex = reader.GetPackedShort();
            toIndex = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedShort(fromIndex);
            writer.PutPackedShort(toIndex);
        }
    }
}
