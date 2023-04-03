using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseReadMailMessage : INetSerializable
    {
        public UITextKeys message;
        public Mail mail;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            if (message == UITextKeys.NONE)
                mail = reader.Get(() => new Mail());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            if (message == UITextKeys.NONE)
                writer.Put(mail);
        }
    }
}
