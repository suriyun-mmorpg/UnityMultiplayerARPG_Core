using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IServerCharacterSkillMessageHandlers
    {
        UniTaskVoid HandleRequestIncreaseCharacterSkillLevel(
            RequestHandlerData requestHandler, RequestIncreaseCharacterSkillLevelMessage request,
            RequestProceedResultDelegate<ResponseIncreaseCharacterSkillLevelMessage> result);
    }
}
