using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestFindGuildsMessage : INetSerializable
    {
        public string guildName;

        public void Deserialize(NetDataReader reader)
        {
            guildName = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(guildName);
        }
    }
}
