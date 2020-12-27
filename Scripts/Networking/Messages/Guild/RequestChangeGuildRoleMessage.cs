using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestChangeGuildRoleMessage : INetSerializable
    {
        public byte guildRole;
        public string name;
        public bool canInvite;
        public bool canKick;
        public byte shareExpPercentage;

        public void Deserialize(NetDataReader reader)
        {
            guildRole = reader.GetByte();
            name = reader.GetString();
            canInvite = reader.GetBool();
            canKick = reader.GetBool();
            shareExpPercentage = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(guildRole);
            writer.Put(name);
            writer.Put(canInvite);
            writer.Put(canKick);
            writer.Put(shareExpPercentage);
        }
    }
}
