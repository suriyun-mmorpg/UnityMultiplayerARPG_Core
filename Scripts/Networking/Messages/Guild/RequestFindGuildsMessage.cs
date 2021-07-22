using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestFindGuildsMessage : INetSerializable
    {
        public string guildName;
        public int skip;
        public int take;
        public GuildListFieldOptions fieldOptions;

        public void Deserialize(NetDataReader reader)
        {
            guildName = reader.GetString();
            skip = reader.GetPackedInt();
            take = reader.GetPackedInt();
            fieldOptions = (GuildListFieldOptions)reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(guildName);
            writer.PutPackedInt(skip);
            writer.PutPackedInt(take);
            writer.PutPackedInt((int)fieldOptions);
        }
    }
}
