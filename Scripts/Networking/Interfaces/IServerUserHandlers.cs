using System.Collections.Generic;

namespace MultiplayerARPG
{
    /// <summary>
    /// These properties and functions will be called at server only
    /// </summary>
    public partial interface IServerUserHandlers
    {
        /// <summary>
        /// Count online characters
        /// </summary>
        int PlayerCharactersCount { get; }

        /// <summary>
        /// Get all online characters
        /// </summary>
        /// <returns></returns>
        IEnumerable<IPlayerCharacterData> GetPlayerCharacters();

        /// <summary>
        /// Get character from server's collection
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="playerCharacter"></param>
        /// <returns></returns>
        bool TryGetPlayerCharacter(long connectionId, out IPlayerCharacterData playerCharacter);

        /// <summary>
        /// Get connection ID by character's ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        bool TryGetConnectionId(string id, out long connectionId);

        /// <summary>
        /// Get character from server's collection
        /// </summary>
        /// <param name="id"></param>
        /// <param name="playerCharacter"></param>
        /// <returns></returns>
        bool TryGetPlayerCharacterById(string id, out IPlayerCharacterData playerCharacter);

        /// <summary>
        /// Get character from server's collection
        /// </summary>
        /// <param name="id"></param>
        /// <param name="playerCharacter"></param>
        /// <returns></returns>
        bool TryGetPlayerCharacterByUserId(string userId, out IPlayerCharacterData playerCharacter);

        /// <summary>
        /// Get character from server's collection
        /// </summary>
        /// <param name="name"></param>
        /// <param name="playerCharacter"></param>
        /// <returns></returns>
        bool TryGetPlayerCharacterByName(string name, out IPlayerCharacterData playerCharacter);

        /// <summary>
        /// Add character to server's collection
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="playerCharacter"></param>
        /// <returns></returns>
        bool AddPlayerCharacter(long connectionId, IPlayerCharacterData playerCharacter);

        /// <summary>
        /// Remove character from server's collection
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        bool RemovePlayerCharacter(long connectionId);

        /// <summary>
        /// Clear server's collection (and other relates variables)
        /// </summary>
        void ClearUsersAndPlayerCharacters();

        /// <summary>
        /// Count online user IDs
        /// </summary>
        int UserIdsCount { get; }

        /// <summary>
        /// Get all online user IDs
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetUserIds();

        /// <summary>
        /// Get user id by connection id
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool TryGetUserId(long connectionId, out string userId);

        /// <summary>
        /// Add user id to server's collection
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool AddUserId(long connectionId, string userId);

        /// <summary>
        /// Remove user id from server's collection
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        bool RemoveUserId(long connectionId);

        /// <summary>
        /// Ban user who own character which its name = `characterName`
        /// </summary>
        /// <param name="characterName"></param>
        /// <param name="days"></param>
        void BanUserByCharacterName(string characterName, int days);

        /// <summary>
        /// Unban user who own character which its name = `characterName`
        /// </summary>
        /// <param name="characterName"></param>
        void UnbanUserByCharacterName(string characterName);

        /// <summary>
        /// Mute character which its name = `characterName`
        /// </summary>
        /// <param name="characterName"></param>
        /// <param name="minutes"></param>
        void MuteCharacterByName(string characterName, int minutes);

        /// <summary>
        /// Unmute user who own character which its name = `characterName`
        /// </summary>
        /// <param name="characterName"></param>
        void UnmuteCharacterByName(string characterName);
    }
}
