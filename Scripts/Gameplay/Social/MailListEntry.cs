using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial class MailListEntry : INetSerializable
    {
        public string Id { get; set; }
        public string SenderName { get; set; }
        public string Title { get; set; }
        public bool IsRead { get; set; }
        public bool IsClaim { get; set; }
        public bool IsDelete { get; set; }
        public int SentTimestamp { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(SenderName);
            writer.Put(Title);
            writer.Put(IsRead);
            writer.Put(IsClaim);
            writer.Put(IsDelete);
            writer.Put(SentTimestamp);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetString();
            SenderName = reader.GetString();
            Title = reader.GetString();
            IsRead = reader.GetBool();
            IsClaim = reader.GetBool();
            IsDelete = reader.GetBool();
            SentTimestamp = reader.GetInt();
        }
    }
}
