namespace MultiplayerARPG
{
    public interface IServerGuildHandlers
    {
        GuildData GetGuild(int id);
        void SetGuild(int id, GuildData guildData);
    }
}
