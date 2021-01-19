using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseGetFriendsMessage : INetSerializable
    {
        public UITextKeys message;
        public SocialCharacterData[] friends;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            friends = reader.GetArray<SocialCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.PutArray(friends);
        }
    }
}