using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseGetFriendsMessage : INetSerializable
    {
        public UITextKeys error;
        public SocialCharacterData[] friends;

        public void Deserialize(NetDataReader reader)
        {
            error = (UITextKeys)reader.GetPackedUShort();
            friends = reader.GetArray<SocialCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)error);
            writer.PutArray(friends);
        }
    }
}