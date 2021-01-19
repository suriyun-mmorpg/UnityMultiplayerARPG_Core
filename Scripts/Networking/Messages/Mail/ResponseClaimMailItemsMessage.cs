using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseClaimMailItemsMessage : INetSerializable
    {
        public UITextKeys message;
        public Mail mail;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            if (message == UITextKeys.NONE)
                mail = reader.GetValue<Mail>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            if (message == UITextKeys.NONE)
                writer.PutValue(mail);
        }
    }
}
