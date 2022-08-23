using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ChatMessage : INetSerializable
    {
        public ChatChannel channel;
        public string message;
        public string sender;
        public string receiver;
        public int guildId;
        public string guildName;
        public int channelId;
        public bool sendByServer;
        public long timestamp;

        public void Deserialize(NetDataReader reader)
        {
            channel = (ChatChannel)reader.GetByte();
            message = reader.GetString();
            sender = reader.GetString();
            receiver = reader.GetString();
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
            writer.Put(sender);
            writer.Put(receiver);
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
