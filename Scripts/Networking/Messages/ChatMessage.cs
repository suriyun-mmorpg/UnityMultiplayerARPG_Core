using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial struct ChatMessage : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            channel = (ChatChannel)reader.GetByte();
            message = reader.GetString();
            senderUserId = reader.GetString();
            senderId = reader.GetString();
            senderName = reader.GetString();
            receiverUserId = reader.GetString();
            receiverId = reader.GetString();
            receiverName = reader.GetString();
            guildId = reader.GetPackedInt();
            if (guildId > 0)
                guildName = reader.GetString();
            if (channel == ChatChannel.Party || channel == ChatChannel.Guild)
                channelId = reader.GetPackedInt();
            sendByServer = reader.GetBool();
            timestamp = reader.GetPackedLong();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)channel);
            writer.Put(message);
            writer.Put(senderUserId);
            writer.Put(senderId);
            writer.Put(senderName);
            writer.Put(receiverUserId);
            writer.Put(receiverId);
            writer.Put(receiverName);
            writer.PutPackedInt(guildId);
            if (guildId > 0)
                writer.Put(guildName);
            if (channel == ChatChannel.Party || channel == ChatChannel.Guild)
                writer.PutPackedInt(channelId);
            writer.Put(sendByServer);
            writer.PutPackedLong(timestamp);
        }
    }
}
