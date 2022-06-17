using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseGuildRequestNotificationMessage : INetSerializable
    {
        public UITextKeys message;
        public int notificationCount;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            if (message == UITextKeys.NONE)
                notificationCount = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            if (message == UITextKeys.NONE)
                writer.Put(notificationCount);
        }
    }
}
