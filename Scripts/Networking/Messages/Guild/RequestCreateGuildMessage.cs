using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestCreateGuildMessage : INetSerializable
    {
        public string characterId;
        public string guildName;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            guildName = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.Put(guildName);
        }
    }
}
