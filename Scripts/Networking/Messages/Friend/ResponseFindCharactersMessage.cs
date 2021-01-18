using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseFindCharactersMessage : INetSerializable
    {
        public UITextKeys error;
        public SocialCharacterData[] characters;

        public void Deserialize(NetDataReader reader)
        {
            error = (UITextKeys)reader.GetPackedUShort();
            characters = reader.GetArray<SocialCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)error);
            writer.PutArray(characters);
        }
    }
}
