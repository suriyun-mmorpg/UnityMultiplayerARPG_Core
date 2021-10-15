using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ChatMessage : INetSerializable
    {
        public ChatChannel channel;
        public string message;
        public string sender;
        public string receiver;
        public int channelId;
        public bool sendByServer;

        public void Deserialize(NetDataReader reader)
        {
            channel = (ChatChannel)reader.GetByte();
            message = reader.GetString();
            sender = reader.GetString();
            receiver = reader.GetString();
            if (channel == ChatChannel.Party || channel == ChatChannel.Guild)
                channelId = reader.GetInt();
            sendByServer = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)channel);
            writer.Put(message);
            writer.Put(sender);
            writer.Put(receiver);
            if (channel == ChatChannel.Party || channel == ChatChannel.Guild)
                writer.Put(channelId);
            writer.Put(sendByServer);
        }
    }
}
