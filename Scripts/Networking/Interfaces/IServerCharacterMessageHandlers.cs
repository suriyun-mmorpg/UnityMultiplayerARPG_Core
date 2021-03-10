using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial interface IServerCharacterMessageHandlers
    {
        UniTaskVoid HandleRequestIncreaseAttributeAmount(
            RequestHandlerData requestHandler, RequestIncreaseAttributeAmountMessage request,
            RequestProceedResultDelegate<ResponseIncreaseAttributeAmountMessage> result);

        UniTaskVoid HandleRequestIncreaseSkillLevel(
            RequestHandlerData requestHandler, RequestIncreaseSkillLevelMessage request,
            RequestProceedResultDelegate<ResponseIncreaseSkillLevelMessage> result);

        UniTaskVoid HandleRequestRespawn(
            RequestHandlerData requestHandler, RequestRespawnMessage request,
            RequestProceedResultDelegate<ResponseRespawnMessage> result);
    }
}
