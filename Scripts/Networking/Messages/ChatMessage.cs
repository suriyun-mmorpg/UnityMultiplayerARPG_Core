using LiteNetLibManager;
using LiteNetLib.Utils;

public class ChatMessage : ILiteNetLibMessage
{
    public ChatChannel channel;
    public string message;
    public string sender;
    public string receiver;

    public void Deserialize(NetDataReader reader)
    {
        channel = (ChatChannel)reader.GetByte();
        message = reader.GetString();
        sender = reader.GetString();
        receiver = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((byte)channel);
        writer.Put(message);
        writer.Put(sender);
        writer.Put(receiver);
    }
}
