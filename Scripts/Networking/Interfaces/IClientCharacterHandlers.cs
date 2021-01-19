using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientCharacterHandlers
    {
        bool RequestIncreaseAttributeAmount(RequestIncreaseAttributeAmountMessage data, ResponseDelegate<ResponseIncreaseAttributeAmountMessage> callback);
        bool RequestIncreaseSkillLevel(RequestIncreaseSkillLevelMessage data, ResponseDelegate<ResponseIncreaseSkillLevelMessage> callback);
    }
}
