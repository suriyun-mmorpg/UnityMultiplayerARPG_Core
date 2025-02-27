using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultServerUserHandlers : MonoBehaviour, IServerUserHandlers
    {
        public static readonly ConcurrentDictionary<long, IPlayerCharacterData> PlayerCharacters = new ConcurrentDictionary<long, IPlayerCharacterData>();
        public static readonly ConcurrentDictionary<string, IPlayerCharacterData> PlayerCharactersById = new ConcurrentDictionary<string, IPlayerCharacterData>();
        public static readonly ConcurrentDictionary<string, IPlayerCharacterData> PlayerCharactersByUserId = new ConcurrentDictionary<string, IPlayerCharacterData>();
        public static readonly ConcurrentDictionary<string, IPlayerCharacterData> PlayerCharactersByName = new ConcurrentDictionary<string, IPlayerCharacterData>();
        public static readonly ConcurrentDictionary<string, long> PlayerCharacterConnectionIds = new ConcurrentDictionary<string, long>();
        public static readonly ConcurrentDictionary<long, string> UserIds = new ConcurrentDictionary<long, string>();

        public int PlayerCharactersCount
        {
            get { return PlayerCharacters.Count; }
        }

        public int UserIdsCount
        {
            get { return UserIds.Count; }
        }

        public IEnumerable<IPlayerCharacterData> GetPlayerCharacters()
        {
            return PlayerCharacters.Values;
        }

        public bool TryGetPlayerCharacter(long connectionId, out IPlayerCharacterData playerCharacter)
        {
            return PlayerCharacters.TryGetValue(connectionId, out playerCharacter);
        }

        public bool TryGetConnectionId(string id, out long connectionId)
        {
            return PlayerCharacterConnectionIds.TryGetValue(id, out connectionId);
        }

        public bool TryGetPlayerCharacterById(string id, out IPlayerCharacterData playerCharacter)
        {
            return PlayerCharactersById.TryGetValue(id, out playerCharacter);
        }

        public bool TryGetPlayerCharacterByUserId(string userId, out IPlayerCharacterData playerCharacter)
        {
            return PlayerCharactersByUserId.TryGetValue(userId, out playerCharacter);
        }

        public bool TryGetPlayerCharacterByName(string name, out IPlayerCharacterData playerCharacter)
        {
            return PlayerCharactersByName.TryGetValue(name, out playerCharacter);
        }

        public bool AddPlayerCharacter(long connectionId, IPlayerCharacterData playerCharacter)
        {
            if (playerCharacter == null || string.IsNullOrEmpty(playerCharacter.Id) || string.IsNullOrEmpty(playerCharacter.UserId) || string.IsNullOrEmpty(playerCharacter.CharacterName))
                return false;
            if (PlayerCharacters.TryAdd(connectionId, playerCharacter))
            {
                PlayerCharactersById.TryAdd(playerCharacter.Id, playerCharacter);
                PlayerCharactersByUserId.TryAdd(playerCharacter.UserId, playerCharacter);
                PlayerCharactersByName.TryAdd(playerCharacter.CharacterName, playerCharacter);
                PlayerCharacterConnectionIds.TryAdd(playerCharacter.Id, connectionId);
                return true;
            }
            return false;
        }

        public bool RemovePlayerCharacter(long connectionId, out string characterId, out string userId)
        {
            IPlayerCharacterData playerCharacter;
            if (PlayerCharacters.TryRemove(connectionId, out playerCharacter))
            {
                characterId = playerCharacter.Id;
                userId = playerCharacter.UserId;
                PlayerCharactersById.TryRemove(characterId, out _);
                PlayerCharactersByUserId.TryRemove(userId, out _);
                PlayerCharactersByName.TryRemove(playerCharacter.CharacterName, out _);
                PlayerCharacterConnectionIds.TryRemove(characterId, out _);
                return true;
            }
            characterId = null;
            userId = null;
            return false;
        }

        public void ClearUsersAndPlayerCharacters()
        {
            PlayerCharacters.Clear();
            PlayerCharactersById.Clear();
            PlayerCharactersByUserId.Clear();
            PlayerCharactersByName.Clear();
            PlayerCharacterConnectionIds.Clear();
            UserIds.Clear();
        }

        public IEnumerable<string> GetUserIds()
        {
            return UserIds.Values;
        }

        public bool TryGetUserId(long connectionId, out string userId)
        {
            return UserIds.TryGetValue(connectionId, out userId);
        }

        public bool AddUserId(long connectionId, string userId)
        {
            return UserIds.TryAdd(connectionId, userId);
        }

        public bool RemoveUserId(long connectionId, out string userId)
        {
            return UserIds.TryRemove(connectionId, out userId);
        }

        public virtual void BanUserByCharacterName(string characterName, int days)
        {
            throw new System.NotImplementedException();
        }

        public virtual void UnbanUserByCharacterName(string characterName)
        {
            throw new System.NotImplementedException();
        }

        public virtual void MuteCharacterByName(string characterName, int minutes)
        {
            long time = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (60 * minutes);
            if (TryGetPlayerCharacterByName(characterName, out IPlayerCharacterData playerCharacter))
                playerCharacter.UnmuteTime = time;
        }

        public virtual void UnmuteCharacterByName(string characterName)
        {
            if (TryGetPlayerCharacterByName(characterName, out IPlayerCharacterData playerCharacter))
                playerCharacter.UnmuteTime = 0;
        }

        public virtual void ChangeUserGold(string userId, int gold)
        {
            if (!TryGetPlayerCharacterByUserId(userId, out IPlayerCharacterData playerCharacter))
                return;
            playerCharacter.UserGold = playerCharacter.UserGold.Increase(gold);
        }

        public virtual void ChangeUserCash(string userId, int cash)
        {
            if (!TryGetPlayerCharacterByUserId(userId, out IPlayerCharacterData playerCharacter))
                return;
            playerCharacter.UserCash = playerCharacter.UserCash.Increase(cash);
        }

        public virtual UniTask<UITextKeys> ValidateCharacterName(string characterName)
        {
            return new UniTask<UITextKeys>(UITextKeys.NONE);
        }
    }
}
