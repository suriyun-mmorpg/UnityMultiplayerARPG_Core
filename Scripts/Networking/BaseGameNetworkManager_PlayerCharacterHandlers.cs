using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
        public static readonly ConcurrentDictionary<long, IPlayerCharacterData> PlayerCharacters = new ConcurrentDictionary<long, IPlayerCharacterData>();
        public static readonly ConcurrentDictionary<string, IPlayerCharacterData> PlayerCharactersById = new ConcurrentDictionary<string, IPlayerCharacterData>();
        public static readonly ConcurrentDictionary<string, IPlayerCharacterData> PlayerCharactersByName = new ConcurrentDictionary<string, IPlayerCharacterData>();

        public int PlayerCharactersCount
        {
            get { return PlayerCharacters.Count; }
        }

        public IEnumerable<IPlayerCharacterData> GetPlayerCharacters()
        {
            return PlayerCharacters.Values;
        }

        public bool TryGetPlayerCharacter(long connectionId, out IPlayerCharacterData playerCharacter)
        {
            return PlayerCharacters.TryGetValue(connectionId, out playerCharacter);
        }

        public bool TryGetPlayerCharacterById(string id, out IPlayerCharacterData playerCharacter)
        {
            return PlayerCharactersById.TryGetValue(id, out playerCharacter);
        }

        public bool TryGetPlayerCharacterByName(string name, out IPlayerCharacterData playerCharacter)
        {
            return PlayerCharactersByName.TryGetValue(name, out playerCharacter);
        }

        public bool AddPlayerCharacter(long connectionId, IPlayerCharacterData playerCharacter)
        {
            if (playerCharacter == null || string.IsNullOrEmpty(playerCharacter.Id) || string.IsNullOrEmpty(playerCharacter.CharacterName))
                return false;
            if (PlayerCharacters.TryAdd(connectionId, playerCharacter))
            {
                PlayerCharactersById.TryAdd(playerCharacter.Id, playerCharacter);
                PlayerCharactersByName.TryAdd(playerCharacter.CharacterName, playerCharacter);
                return true;
            }
            return false;
        }

        public bool RemovePlayerCharacter(long connectionId)
        {
            IPlayerCharacterData playerCharacter;
            if (PlayerCharacters.TryRemove(connectionId, out playerCharacter))
            {
                PlayerCharactersById.TryRemove(playerCharacter.Id, out _);
                PlayerCharactersByName.TryRemove(playerCharacter.CharacterName, out _);
                return true;
            }
            return false;
        }

        public void ClearPlayerCharacters()
        {
            PlayerCharacters.Clear();
            PlayerCharactersById.Clear();
            PlayerCharactersByName.Clear();
        }
    }
}
