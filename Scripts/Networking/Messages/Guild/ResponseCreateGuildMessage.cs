using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseCreateGuildMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            InternalServerError,
            NotAvailable,
            NotAllowed,
            NotLoggedIn,
            NameTooShort,
            NameTooLong,
            NameExisted,
            AlreadyJoined,
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
