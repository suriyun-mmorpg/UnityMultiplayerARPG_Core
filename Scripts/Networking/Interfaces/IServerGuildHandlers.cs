using Cysharp.Threading.Tasks;

namespace MultiplayerARPG
{
    public interface IServerGuildHandlers
    {
        UniTask<bool> TryGetGuild(int guildId, out GuildData guildData);
        UniTask SetGuild(int guildId, GuildData guildData);
        UniTask DeleteGuild(int guildId);
        UniTask<GuildData> CreateGuild(IPlayerCharacterData playerCharacter, string guildName);
        UniTask<bool> HasInvitation(int guildId, string characterId);
        UniTask AppendInvitation(int guildId, string characterId);
        UniTask DeleteInvitation(int guildId, string characterId);
    }
}
