using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestAcceptPartyInvitationMessage : INetSerializable
    {
        public string characterId;
        public int partyId;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            partyId = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.PutPackedInt(partyId);
        }
    }
}
