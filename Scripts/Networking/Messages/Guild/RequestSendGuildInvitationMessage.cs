using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestSendGuildInvitationMessage : INetSerializable
    {
        public string characterId;
        public string inviteeId;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            inviteeId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.Put(inviteeId);
        }
    }
}
