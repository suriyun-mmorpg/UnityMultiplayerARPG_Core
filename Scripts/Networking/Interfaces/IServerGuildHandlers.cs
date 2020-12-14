namespace MultiplayerARPG
{
    public interface IServerGuildHandlers
    {
        bool TryGetGuild(int guildId, out GuildData guildData);
        void SetGuild(int guildId, GuildData guildData);
        void DeleteGuild(int guildId);
        bool HasInvitation(int guildId, string characterId);
        void AppendInvitation(int guildId, string characterId);
        void DeleteInvitation(int guildId, string characterId);
    }
}
