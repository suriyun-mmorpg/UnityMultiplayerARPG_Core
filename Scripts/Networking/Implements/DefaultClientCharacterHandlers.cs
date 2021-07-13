using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultClientCharacterHandlers : MonoBehaviour, IClientCharacterHandlers
    {
        public static readonly Dictionary<string, IPlayerCharacterData> SubscribedPlayerCharacters = new Dictionary<string, IPlayerCharacterData>();

        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public bool RequestIncreaseAttributeAmount(RequestIncreaseAttributeAmountMessage data, ResponseDelegate<ResponseIncreaseAttributeAmountMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.IncreaseAttributeAmount, data, responseDelegate: callback);
        }

        public bool RequestIncreaseSkillLevel(RequestIncreaseSkillLevelMessage data, ResponseDelegate<ResponseIncreaseSkillLevelMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.IncreaseSkillLevel, data, responseDelegate: callback);
        }

        public bool RequestRespawn(RequestRespawnMessage data, ResponseDelegate<ResponseRespawnMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.Respawn, data, responseDelegate: callback);
        }

        public void SubscribePlayerCharacter(string characterId, IPlayerCharacterData playerCharacter)
        {
            SubscribedPlayerCharacters[characterId] = playerCharacter;
        }

        public void UnsubscribePlayerCharacter(string characterId)
        {
            SubscribedPlayerCharacters.Remove(characterId);
        }

        public bool TryGetSubscribedPlayerCharacter(string characterId, out IPlayerCharacterData playerCharacter)
        {
            return SubscribedPlayerCharacters.TryGetValue(characterId, out playerCharacter);
        }

        public void ClearSubscribedPlayerCharacters()
        {
            SubscribedPlayerCharacters.Clear();
        }
    }
}
