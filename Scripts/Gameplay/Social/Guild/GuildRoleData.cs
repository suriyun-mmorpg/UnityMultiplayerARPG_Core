using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial struct GuildRoleData : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            roleName = reader.GetString();
            canInvite = reader.GetBool();
            canKick = reader.GetBool();
            shareExpPercentage = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(roleName);
            writer.Put(canInvite);
            writer.Put(canKick);
            writer.Put(shareExpPercentage);
        }
    }
}
