using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseFindCharactersMessage : INetSerializable
    {
        public UITextKeys message;
        public SocialCharacterData[] characters;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            characters = reader.GetArray<SocialCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.PutArray(characters);
        }
    }
}
