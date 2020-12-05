using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class ResponseReadMailMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            NotAvailable,
            NotAllowed,
        }
        public Error error;
        public Mail mail;

        public void Deserialize(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            if (error == Error.None)
                mail = reader.GetValue<Mail>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
            if (error == Error.None)
                writer.PutValue(mail);
        }
    }
}
