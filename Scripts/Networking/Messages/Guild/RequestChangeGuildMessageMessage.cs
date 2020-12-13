using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestChangeGuildMessageMessage : INetSerializable
    {
        public string characterId;
        public string message;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            message = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.Put(message);
        }
    }
}