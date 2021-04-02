using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseGuildListMessage : INetSerializable
    {
        public UITextKeys message;
        public GuildListEntry[] guilds;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            guilds = reader.GetArray<GuildListEntry>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.PutArray(guilds);
        }
    }
}
