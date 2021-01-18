using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseClaimMailItemsMessage : INetSerializable
    {
        public UITextKeys error;
        public Mail mail;

        public void Deserialize(NetDataReader reader)
        {
            error = (UITextKeys)reader.GetPackedUShort();
            if (error == UITextKeys.NONE)
                mail = reader.GetValue<Mail>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)error);
            if (error == UITextKeys.NONE)
                writer.PutValue(mail);
        }
    }
}
