using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial class MailListEntry : INetSerializable
    {
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(SenderName);
            writer.Put(Title);
            writer.Put(IsRead);
            writer.Put(IsClaim);
            writer.PutPackedLong(SentTimestamp);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetString();
            SenderName = reader.GetString();
            Title = reader.GetString();
            IsRead = reader.GetBool();
            IsClaim = reader.GetBool();
            SentTimestamp = reader.GetPackedLong();
        }
    }
}
