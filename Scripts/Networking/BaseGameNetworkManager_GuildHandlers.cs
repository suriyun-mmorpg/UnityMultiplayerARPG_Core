using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
        public const int GuildInvitationDuration = 10000;
        public static readonly ConcurrentDictionary<int, GuildData> Guilds = new ConcurrentDictionary<int, GuildData>();
        public static readonly ConcurrentDictionary<long, GuildData> UpdatingGuildMembers = new ConcurrentDictionary<long, GuildData>();
        public static readonly ConcurrentDictionary<string, int> GuildInvitations = new ConcurrentDictionary<string, int>();

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
            return GuildInvitations.ContainsKey(GetGuildInvitationId(guildId, characterId));
        }

        public void AppendGuildInvitation(int guildId, string characterId)
        {
            RemoveGuildInvitation(guildId, characterId);
            GuildInvitations.TryAdd(GetGuildInvitationId(guildId, characterId), guildId);
            DelayRemoveGuildInvitation(guildId, characterId).Forget();
        }

        public void RemoveGuildInvitation(int guildId, string characterId)
        {
            GuildInvitations.TryRemove(GetGuildInvitationId(guildId, characterId), out _);
        }

        public void ClearGuild()
        {
            Guilds.Clear();
            UpdatingGuildMembers.Clear();
            GuildInvitations.Clear();
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

        private string GetGuildInvitationId(int guildId, string characterId)
        {
            return $"{guildId}_{characterId}";
        }

        private async UniTaskVoid DelayRemoveGuildInvitation(int partyId, string characterId)
        {
            await UniTask.Delay(GuildInvitationDuration);
            RemoveGuildInvitation(partyId, characterId);
        }
    }
}
