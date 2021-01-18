using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientCharacterAttributeHandlers
    {
        bool RequestIncreaseCharacterAttributeAmount(RequestIncreaseCharacterAttributeAmountMessage data, ResponseDelegate<ResponseIncreaseCharacterAttributeAmountMessage> callback);
    }
}
