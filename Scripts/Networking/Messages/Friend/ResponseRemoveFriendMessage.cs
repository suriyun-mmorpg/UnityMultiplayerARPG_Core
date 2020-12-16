using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseRemoveFriendMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            NotAvailable,
            CharacterNotFound,
            FriendNotFound,
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
