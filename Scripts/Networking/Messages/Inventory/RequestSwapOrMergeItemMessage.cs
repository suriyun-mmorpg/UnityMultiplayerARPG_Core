using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestSwapOrMergeItemMessage : INetSerializable
    {
        public string characterId;
        public short fromIndex;
        public short toIndex;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            fromIndex = reader.GetPackedShort();
            toIndex = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.PutPackedShort(fromIndex);
            writer.PutPackedShort(toIndex);
        }
    }
}
