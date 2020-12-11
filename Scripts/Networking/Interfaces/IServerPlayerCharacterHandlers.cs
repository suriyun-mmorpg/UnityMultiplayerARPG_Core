using System.Collections.Generic;

namespace MultiplayerARPG
{
    /// <summary>
    /// These properties and functions will be called at server only
    /// </summary>
    public interface IServerPlayerCharacterHandlers
    {
        int PlayerCharactersCount { get; }
        IEnumerable<IPlayerCharacterData> GetPlayerCharacters();
        bool TryGetPlayerCharacter(long connectionId, out IPlayerCharacterData playerCharacter);
        bool TryGetPlayerCharacterById(string id, out IPlayerCharacterData playerCharacter);
        bool TryGetPlayerCharacterByName(string name, out IPlayerCharacterData playerCharacter);
        bool AddPlayerCharacter(long connectionId, IPlayerCharacterData playerCharacter);
        bool RemovePlayerCharacter(long connectionId);
        void ClearPlayerCharacters();
    }
}
