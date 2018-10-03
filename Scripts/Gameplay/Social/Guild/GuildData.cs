using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class GuildData : SocialGroupData
    {
        public const byte LeaderRole = 0;

        public string guildName { get; private set; }
        public string leaderName { get; private set; }
        public int level;
        public int exp;
        public int skillPoint;
        public string guildMessage;
        protected List<GuildMemberRole> roles;
        protected Dictionary<string, byte> memberRoles;

        public byte LowestMemberRole
        {
            get
            {
                if (roles == null || roles.Count < 2)
                    return 1;
                return (byte)(roles.Count - 1);
            }
        }

        public GuildData(int id, string guildName, string leaderId, string leaderName)
            : base(id, leaderId)
        {
            this.guildName = guildName;
            this.leaderName = leaderName;
            level = 1;
            exp = 0;
            skillPoint = 0;
            guildMessage = string.Empty;
            roles = new List<GuildMemberRole>(SystemSetting.GuildMemberRoles);
            memberRoles = new Dictionary<string, byte>();
        }

        public GuildData(int id, string guildName, BasePlayerCharacterEntity leaderCharacterEntity)
            : this(id, guildName, leaderCharacterEntity.Id, leaderCharacterEntity.CharacterName)
        {
            AddMember(leaderCharacterEntity);
        }

        public void AddMember(BasePlayerCharacterEntity playerCharacterEntity, byte guildRole)
        {
            AddMember(CreateMemberData(playerCharacterEntity), guildRole);
        }

        public void AddMember(SocialCharacterData memberData, byte guildRole)
        {
            base.AddMember(memberData);
            SetMemberRole(memberData.id, guildRole);
        }

        public void UpdateMember(SocialCharacterData memberData, byte guildRole)
        {
            base.UpdateMember(memberData);
            SetMemberRole(memberData.id, guildRole);
        }

        public override void AddMember(SocialCharacterData memberData)
        {
            base.AddMember(memberData);
            SetMemberRole(memberData.id, IsLeader(memberData.id) ? LeaderRole : LowestMemberRole);
        }

        public override bool RemoveMember(string characterId)
        {
            memberRoles.Remove(characterId);
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
            if (!members.ContainsKey(characterId))
            {
                guildRole = 0;
                return 0;
            }

            if (IsLeader(characterId))
            {
                guildRole = LeaderRole;
                if (!IsRoleAvailable(guildRole))
                    return (GuildMemberFlags.IsLeader | GuildMemberFlags.CanInvite | GuildMemberFlags.CanKick);
                return GuildMemberFlags.IsLeader | GetGuildFlags(guildRole);
            }
            else
            {
                guildRole = LowestMemberRole;
                if (!IsRoleAvailable(guildRole))
                    return 0;
                if (!memberRoles.TryGetValue(characterId, out guildRole) || !IsRoleAvailable(guildRole))
                    guildRole = LowestMemberRole;
                return GetGuildFlags(guildRole);
            }
        }

        private GuildMemberFlags GetGuildFlags(byte guildRole)
        {
            var role = roles[guildRole];
            return ((role.canInvite ? GuildMemberFlags.CanInvite : 0) | (role.canKick ? GuildMemberFlags.CanKick : 0));
        }

        public bool TryGetMemberRole(string characterId, out byte role)
        {
            return memberRoles.TryGetValue(characterId, out role);
        }

        public void SetMemberRole(string characterId, byte guildRole)
        {
            if (members.ContainsKey(characterId))
            {
                if (!IsRoleAvailable(guildRole))
                    guildRole = IsLeader(characterId) ? LeaderRole : LowestMemberRole;
                memberRoles[characterId] = guildRole;
            }
        }

        public bool IsRoleAvailable(byte guildRole)
        {
            return roles != null && guildRole < roles.Count;
        }

        public GuildMemberRole GetRole(byte guildRole)
        {
            if (!IsRoleAvailable(guildRole))
            {
                if (guildRole == LeaderRole)
                    return new GuildMemberRole() { name = "Master", canInvite = true, canKick = true };
                else
                    return new GuildMemberRole() { name = "Member", canInvite = false, canKick = false };
            }
            return roles[guildRole];
        }

        public void SetRole(byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            SetRole(guildRole, new GuildMemberRole()
            {
                name = name,
                canInvite = canInvite,
                canKick = canKick,
                shareExpPercentage = shareExpPercentage,
            });
        }

        public void SetRole(byte guildRole, GuildMemberRole role)
        {
            roles[guildRole] = role;
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
