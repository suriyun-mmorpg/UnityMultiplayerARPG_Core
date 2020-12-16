using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestFindCharactersMessage : INetSerializable
    {
        public string characterName;

        public void Deserialize(NetDataReader reader)
        {
            characterName = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterName);
        }
    }
}
