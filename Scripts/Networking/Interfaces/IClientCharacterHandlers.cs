using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial interface IClientCharacterHandlers
    {
        bool RequestIncreaseAttributeAmount(RequestIncreaseAttributeAmountMessage data, ResponseDelegate<ResponseIncreaseAttributeAmountMessage> callback);
        bool RequestIncreaseSkillLevel(RequestIncreaseSkillLevelMessage data, ResponseDelegate<ResponseIncreaseSkillLevelMessage> callback);
        bool RequestRespawn(RequestRespawnMessage data, ResponseDelegate<ResponseRespawnMessage> callback);
        void SubscribePlayerCharacter(string characterId, IPlayerCharacterData playerCharacter);
        void UnsubscribePlayerCharacter(string characterId);
        bool TryGetSubscribedPlayerCharacter(string characterId, out IPlayerCharacterData playerCharacter);
        void ClearSubscribedPlayerCharacters();
    }
}
