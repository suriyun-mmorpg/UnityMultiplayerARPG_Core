using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseAddFriendMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            InternalServerError,
            NotAvailable,
            NotLoggedIn,
            FriendNotFound,
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
