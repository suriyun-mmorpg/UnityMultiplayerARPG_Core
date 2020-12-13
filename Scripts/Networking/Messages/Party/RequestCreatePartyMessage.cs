using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestCreatePartyMessage : INetSerializable
    {
        public string characterId;
        public string partyName;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            partyName = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.Put(partyName);
        }
    }
}
