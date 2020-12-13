using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseCreateGuildMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            NotAvailable,
            NotAllowed,
            CharacterNotFound,
            NameTooShort,
            NameTooLong,
            AlreadyJoined,
            InternalServerError,
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
