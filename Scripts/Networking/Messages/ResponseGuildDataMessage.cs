using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class ResponseGuildDataMessage : ResponseSocialGroupDataMessage
    {
        public string guildName;
        public string leaderName;
        public int level;
        public int exp;
        public int skillPoint;
        public string message;
        public GuildRole[] roles;
        public byte[] memberRoles;

        public override void DeserializeData(NetDataReader reader)
        {
            guildName = reader.GetString();
            leaderName = reader.GetString();
            level = reader.GetInt();
            exp = reader.GetInt();
            skillPoint = reader.GetInt();
            message = reader.GetString();
            // Put roles
            var length = reader.GetByte();
            var roles = new GuildRole[length];
            if (length > 0)
            {
                for (var i = 0; i < length; ++i)
                {
                    var entry = new GuildRole();
                    entry.roleName = reader.GetString();
                    entry.canInvite = reader.GetBool();
                    entry.canKick = reader.GetBool();
                    entry.shareExpPercentage = reader.GetByte();
                    roles[i] = entry;
                }
            }
            this.roles = roles;
            // Put member roles
            length = reader.GetByte();
            var memberRoles = new byte[length];
            if (length > 0)
            {
                for (var i = 0; i < length; ++i)
                {
                    memberRoles[i] = reader.GetByte();
                }
            }
            this.memberRoles = memberRoles;

            base.DeserializeData(reader);
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(guildName);
            writer.Put(leaderName);
            writer.Put(level);
            writer.Put(exp);
            writer.Put(skillPoint);
            writer.Put(message);
            // Put roles
            var length = (byte)(roles == null ? 0 : roles.Length);
            writer.Put(length);
            if (length > 0)
            {
                for (var i = 0; i < length; ++i)
                {
                    var entry = roles[i];
                    writer.Put(entry.roleName);
                    writer.Put(entry.canInvite);
                    writer.Put(entry.canKick);
                    writer.Put(entry.shareExpPercentage);
                }
            }
            // Put member roles
            length = (byte)(memberRoles == null ? 0 : memberRoles.Length);
            writer.Put(length);
            if (length > 0)
            {
                for (var i = 0; i < length; ++i)
                {
                    writer.Put(memberRoles[i]);
                }
            }

            base.SerializeData(writer);
        }
    }
}
