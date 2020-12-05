using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class ResponseDeleteMailMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            NotAvailable,
            NotAllowed,
        }
        public Error error;

        public void Deserialize(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
        }
    }
}
