using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseSendPartyInvitationMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            InternalServerError,
            NotAvailable,
            NotAllowed,
            NotLoggedIn,
            InviteeNotFound,
            NotJoined,
            InviteeNotAvailable,
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
