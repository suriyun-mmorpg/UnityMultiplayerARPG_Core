using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestChangeGuildOptionIdMessage : INetSerializable
    {
        public int optionId;

        public void Deserialize(NetDataReader reader)
        {
            optionId = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(optionId);
        }
    }
}