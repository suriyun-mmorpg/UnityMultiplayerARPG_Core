using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IServerCharacterAttributeMessageHandlers
    {
        UniTaskVoid HandleRequestIncreaseCharacterAttributeAmount(
            RequestHandlerData requestHandler, RequestIncreaseCharacterAttributeAmountMessage request,
            RequestProceedResultDelegate<ResponseIncreaseCharacterAttributeAmountMessage> result);
    }
}
