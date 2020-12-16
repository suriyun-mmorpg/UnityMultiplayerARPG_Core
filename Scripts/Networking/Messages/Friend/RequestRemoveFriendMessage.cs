using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestRemoveFriendMessage : INetSerializable
    {
        public string characterId;
        public string friendId;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            friendId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.Put(friendId);
        }
    }
}
