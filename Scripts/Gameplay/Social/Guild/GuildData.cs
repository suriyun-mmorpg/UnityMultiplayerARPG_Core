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

        public GuildData(int id, string guildName, string leaderId, string leaderName)
            : base(id, leaderId)
        {
            this.guildName = guildName;
            this.leaderName = leaderName;
            level = 1;
            exp = 0;
            skillPoint = 0;
            guildMessage = string.Empty;
        }

        public GuildData(int id, string guildName, BasePlayerCharacterEntity leaderCharacterEntity)
            : this(id, guildName, leaderCharacterEntity.Id, leaderCharacterEntity.CharacterName)
        {
            AddMember(leaderCharacterEntity);
        }

        public override byte GetMemberFlags(SocialCharacterData memberData)
        {
            return (byte)GetGuildMemberFlags(memberData.id);
        }

        private GuildMemberFlags GetGuildLeaderFlags()
        {
            if (SystemSetting.guildMemberRoles == null || SystemSetting.guildMemberRoles.Length <= 0)
                return (GuildMemberFlags.IsLeader | GuildMemberFlags.CanInvite | GuildMemberFlags.CanKick);
            return GuildMemberFlags.IsLeader | GetGuildFlags(0);
        }

        private GuildMemberFlags GetGuildMemberFlags()
        {
            if (SystemSetting.guildMemberRoles == null || SystemSetting.guildMemberRoles.Length <= 1)
                return 0;
            return GetGuildFlags(SystemSetting.guildMemberRoles.Length - 1);
        }

        public GuildMemberFlags GetGuildMemberFlags(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (IsLeader(playerCharacterEntity))
                return GetGuildLeaderFlags();
            else
                return GetGuildMemberFlags();
        }

        public GuildMemberFlags GetGuildMemberFlags(string characterId)
        {
            if (IsLeader(characterId))
                return GetGuildLeaderFlags();
            else
                return GetGuildMemberFlags();
        }

        public GuildMemberFlags GetGuildFlags(int roleIdx)
        {
            var role = SystemSetting.guildMemberRoles[roleIdx];
            return ((role.canInvite ? GuildMemberFlags.CanInvite : 0) | (role.canKick ? GuildMemberFlags.CanKick : 0));
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
