using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseGetFriendRequestsMessage : INetSerializable
    {
        public UITextKeys message;
        public SocialCharacterData[] friendRequests;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            friendRequests = reader.GetArray<SocialCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.PutArray(friendRequests);
        }
    }
}