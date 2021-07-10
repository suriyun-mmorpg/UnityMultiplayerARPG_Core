using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct GuildInvitationData : INetSerializable
    {
        public string InviterId { get; set; }
        public string InviterName { get; set; }
        public short InviterLevel { get; set; }
        public int GuildId { get; set; }
        public string GuildName { get; set; }
        public short GuildLevel { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            InviterId = reader.GetString();
            InviterName = reader.GetString();
            InviterLevel = reader.GetPackedShort();
            GuildId = reader.GetPackedInt();
            GuildName = reader.GetString();
            GuildLevel = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(InviterId);
            writer.Put(InviterName);
            writer.PutPackedShort(InviterLevel);
            writer.PutPackedInt(GuildId);
            writer.Put(GuildName);
            writer.PutPackedShort(GuildLevel);
        }
    }
}
