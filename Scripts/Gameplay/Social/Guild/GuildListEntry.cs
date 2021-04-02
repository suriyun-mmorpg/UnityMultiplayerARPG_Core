using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class GuildListEntry : INetSerializable
    {
        public int Id { get; set; }
        public string GuildName { get; set; }
        public short Level { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(GuildName);
            writer.Put(Level);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetInt();
            GuildName = reader.GetString();
            Level = reader.GetShort();
        }
    }
}
