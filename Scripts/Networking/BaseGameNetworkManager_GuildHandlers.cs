using System.Collections.Concurrent;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
        public static readonly ConcurrentDictionary<int, GuildData> Guilds = new ConcurrentDictionary<int, GuildData>();
        public static readonly ConcurrentDictionary<long, GuildData> UpdatingGuildMembers = new ConcurrentDictionary<long, GuildData>();

        public bool TryGetGuild(int guildId, out GuildData guildData)
        {
            return Guilds.TryGetValue(guildId, out guildData);
        }

        public void SetGuild(int guildId, GuildData guildData)
        {
            if (Guilds.ContainsKey(guildId))
                Guilds[guildId] = guildData;
            else
                Guilds.TryAdd(guildId, guildData);
        }

        public void RemoveGuild(int guildId)
        {
            Guilds.TryRemove(guildId, out _);
        }

        public bool HasGuildInvitation(int guildId, string characterId)
        {
            throw new System.NotImplementedException();
        }

        public void AppendGuildInvitation(int guildId, string characterId)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveGuildInvitation(int guildId, string characterId)
        {
            throw new System.NotImplementedException();
        }

        public void ClearGuild()
        {
            Guilds.Clear();
            UpdatingGuildMembers.Clear();
        }

        public void IncreaseGuildExp(IPlayerCharacterData playerCharacter, int exp)
        {
            ValidateGuildRequestResult validateResult = this.CanIncreaseGuildExp(playerCharacter, exp);
            if (!validateResult.IsSuccess)
                return;
            validateResult.Guild = GameInstance.Singleton.SocialSystemSetting.IncreaseGuildExp(validateResult.Guild, exp);
            SetGuild(validateResult.GuildId, validateResult.Guild);
            SendGuildLevelExpSkillPointToClients(validateResult.Guild);
        }
    }
}
