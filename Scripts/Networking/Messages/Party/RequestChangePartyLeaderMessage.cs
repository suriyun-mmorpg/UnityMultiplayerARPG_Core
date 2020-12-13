using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestChangePartyLeaderMessage : INetSerializable
    {
        public string characterId;
        public string memberId;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            memberId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.Put(memberId);
        }
    }
}