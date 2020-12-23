using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseFindCharactersMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            InternalServerError,
            NotAvailable,
            NotLoggedIn,
        }
        public Error error;
        public SocialCharacterData[] characters;

        public void Deserialize(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            characters = reader.GetArray<SocialCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
            writer.PutArray(characters);
        }
    }
}
