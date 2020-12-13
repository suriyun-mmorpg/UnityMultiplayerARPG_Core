using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestChangeMemberGuildRoleMessage : INetSerializable
    {
        public string characterId;
        public string memberId;
        public byte guildRole;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            memberId = reader.GetString();
            guildRole = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.Put(memberId);
            writer.Put(guildRole);
        }
    }
}
