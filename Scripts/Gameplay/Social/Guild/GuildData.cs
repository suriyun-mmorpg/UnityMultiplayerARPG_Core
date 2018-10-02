using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class GuildData : SocialGroupData
    {
        public string guildName { get; private set; }
        public string leaderName { get; private set; }
        public int level;
        public int exp;
        public int skillPoint;
        public string guildMessage;
        protected Dictionary<string, byte> roles;

        public GuildData(int id, string guildName, string leaderId, string leaderName)
            : base(id, leaderId)
        {
            this.guildName = guildName;
            this.leaderName = leaderName;
            level = 1;
            exp = 0;
            skillPoint = 0;
            guildMessage = string.Empty;
            roles = new Dictionary<string, byte>();
        }

        public GuildData(int id, string guildName, BasePlayerCharacterEntity leaderCharacterEntity)
            : this(id, guildName, leaderCharacterEntity.Id, leaderCharacterEntity.CharacterName)
        {
            AddMember(leaderCharacterEntity);
        }

        public void AddMember(SocialCharacterData memberData, byte guildRole)
        {
            AddMember(memberData);
            if (members.ContainsKey(memberData.id))
                roles[memberData.id] = guildRole;
        }

        public void UpdateMember(SocialCharacterData memberData, byte guildRole)
        {
            UpdateMember(memberData);
            if (members.ContainsKey(memberData.id))
                roles[memberData.id] = guildRole;
        }

        public override bool RemoveMember(string characterId)
        {
            roles.Remove(characterId);
            return base.RemoveMember(characterId);
        }

        public override byte GetMemberFlags(SocialCharacterData memberData)
        {
            byte guildRole;
            return (byte)GetGuildMemberFlagsAndRole(memberData.id, out guildRole);
        }

        public GuildMemberFlags GetGuildMemberFlagsAndRole(BasePlayerCharacterEntity playerCharacterEntity, out byte guildRole)
        {
            return GetGuildMemberFlagsAndRole(playerCharacterEntity.Id, out guildRole);
        }

        public GuildMemberFlags GetGuildMemberFlagsAndRole(string characterId, out byte guildRole)
        {
            if (IsLeader(characterId))
            {
                guildRole = 0;
                if (SystemSetting.guildMemberRoles == null || SystemSetting.guildMemberRoles.Length <= guildRole)
                    return (GuildMemberFlags.IsLeader | GuildMemberFlags.CanInvite | GuildMemberFlags.CanKick);
                return GuildMemberFlags.IsLeader | GetGuildFlags(guildRole);
            }
            else
            {
                guildRole = 1;
                if (SystemSetting.guildMemberRoles == null || SystemSetting.guildMemberRoles.Length <= guildRole)
                    return 0;
                if (!roles.TryGetValue(characterId, out guildRole) || SystemSetting.guildMemberRoles.Length <= guildRole)
                    guildRole = (byte)(SystemSetting.guildMemberRoles.Length - 1);
                return GetGuildFlags(guildRole);
            }
        }

        public GuildMemberFlags GetGuildFlags(int roleIdx)
        {
            var role = SystemSetting.guildMemberRoles[roleIdx];
            return ((role.canInvite ? GuildMemberFlags.CanInvite : 0) | (role.canKick ? GuildMemberFlags.CanKick : 0));
        }

        public bool TryGetRole(string characterId, out byte role)
        {
            return roles.TryGetValue(characterId, out role);
        }

        public static bool IsLeader(GuildMemberFlags flags)
        {
            return (flags & GuildMemberFlags.IsLeader) != 0;
        }

        public static bool CanInvite(GuildMemberFlags flags)
        {
            return (flags & GuildMemberFlags.CanInvite) != 0;
        }

        public static bool CanKick(GuildMemberFlags flags)
        {
            return (flags & GuildMemberFlags.CanKick) != 0;
        }
    }
}
