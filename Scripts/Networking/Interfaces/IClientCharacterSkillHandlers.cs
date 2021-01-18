using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientCharacterSkillHandlers
    {
        bool RequestIncreaseCharacterSkillLevel(RequestIncreaseCharacterSkillLevelMessage data, ResponseDelegate<ResponseIncreaseCharacterSkillLevelMessage> callback);
    }
}
