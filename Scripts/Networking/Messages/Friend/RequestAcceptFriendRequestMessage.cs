using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestAcceptFriendRequestMessage : INetSerializable
    {
        public string friendId;

        public void Deserialize(NetDataReader reader)
        {
            friendId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(friendId);
        }
    }
}
