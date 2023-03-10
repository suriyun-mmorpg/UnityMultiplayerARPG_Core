using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial class Mail : INetSerializable
    {
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(EventId);
            writer.Put(SenderId);
            writer.Put(SenderName);
            writer.Put(ReceiverId);
            writer.Put(Title);
            writer.Put(Content);
            writer.PutPackedInt(Gold);
            writer.PutPackedInt(Cash);
            writer.Put(WriteCurrencies());
            writer.Put(WriteItems());
            writer.Put(IsRead);
            writer.PutPackedLong(ReadTimestamp);
            writer.Put(IsClaim);
            writer.PutPackedLong(ClaimTimestamp);
            writer.Put(IsDelete);
            writer.PutPackedLong(DeleteTimestamp);
            writer.PutPackedLong(SentTimestamp);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetString();
            EventId = reader.GetString();
            SenderId = reader.GetString();
            SenderName = reader.GetString();
            ReceiverId = reader.GetString();
            Title = reader.GetString();
            Content = reader.GetString();
            Gold = reader.GetPackedInt();
            Cash = reader.GetPackedInt();
            ReadCurrencies(reader.GetString());
            ReadItems(reader.GetString());
            IsRead = reader.GetBool();
            ReadTimestamp = reader.GetPackedLong();
            IsClaim = reader.GetBool();
            ClaimTimestamp = reader.GetPackedLong();
            IsDelete = reader.GetBool();
            DeleteTimestamp = reader.GetPackedLong();
            SentTimestamp = reader.GetPackedLong();
        }
    }
}
