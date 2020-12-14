using System.Collections.Generic;

namespace MultiplayerARPG
{
    /// <summary>
    /// These properties and functions will be called at server only
    /// </summary>
    public interface IServerPlayerCharacterHandlers
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
        /// Get character from server's collection
        /// </summary>
        /// <param name="id"></param>
        /// <param name="playerCharacter"></param>
        /// <returns></returns>
        bool TryGetPlayerCharacterById(string id, out IPlayerCharacterData playerCharacter);

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
        void ClearPlayerCharacters();
    }
}
