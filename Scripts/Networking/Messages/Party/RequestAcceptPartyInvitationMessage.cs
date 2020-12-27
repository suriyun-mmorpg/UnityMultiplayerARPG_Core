using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestAcceptPartyInvitationMessage : INetSerializable
    {
        public int partyId;

        public void Deserialize(NetDataReader reader)
        {
            partyId = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(partyId);
        }
    }
}
