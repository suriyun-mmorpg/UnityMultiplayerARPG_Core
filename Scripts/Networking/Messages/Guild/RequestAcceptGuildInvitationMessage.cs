using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestAcceptGuildInvitationMessage : INetSerializable
    {
        public string characterId;
        public int guildId;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            guildId = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.PutPackedInt(guildId);
        }
    }
}
