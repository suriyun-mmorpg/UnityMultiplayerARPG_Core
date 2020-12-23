using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseGetFriendsMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            InternalServerError,
            NotAvailable,
            NotLoggedIn,
        }
        public Error error;
        public SocialCharacterData[] friends;

        public void Deserialize(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            friends = reader.GetArray<SocialCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
            writer.PutArray(friends);
        }
    }
}