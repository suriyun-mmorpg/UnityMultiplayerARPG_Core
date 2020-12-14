namespace MultiplayerARPG
{
    /// <summary>
    /// These properties and functions will be called at server only
    /// </summary>
    public interface IServerGuildHandlers
    {
        /// <summary>
        /// Get guild from server's collection
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="guildData"></param>
        /// <returns></returns>
        bool TryGetGuild(int guildId, out GuildData guildData);

        /// <summary>
        /// Set guild to server's collection
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="guildData"></param>
        void SetGuild(int guildId, GuildData guildData);

        /// <summary>
        /// Remove guild from server's collection
        /// </summary>
        /// <param name="guildId"></param>
        void RemoveGuild(int guildId);

        /// <summary>
        /// Find invitation
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="characterId"></param>
        /// <returns></returns>
        bool HasGuildInvitation(int guildId, string characterId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="characterId"></param>
        void AppendGuildInvitation(int guildId, string characterId);

        /// <summary>
        /// Append invitation
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="characterId"></param>
        void RemoveGuildInvitation(int guildId, string characterId);

        /// <summary>
        /// Clear server's collection (and other relates variables)
        /// </summary>
        void ClearGuild();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerCharacter"></param>
        /// <param name="exp"></param>
        void IncreaseGuildExp(IPlayerCharacterData playerCharacter, int exp);
    }
}
