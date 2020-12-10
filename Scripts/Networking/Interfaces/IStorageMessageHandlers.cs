using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IStorageMessageHandlers
    {
        IServerPlayerCharacterHandlers ServerPlayerCharacterHandlers { get; set; }

        UniTaskVoid HandleRequestMoveItemToStorage(
            RequestHandlerData requestHandler, RequestMoveItemToStorageMessage request,
            RequestProceedResultDelegate<ResponseMoveItemToStorageMessage> result);

        UniTaskVoid HandleRequestMoveItemFromStorage(
            RequestHandlerData requestHandler, RequestMoveItemFromStorageMessage request,
            RequestProceedResultDelegate<ResponseMoveItemFromStorageMessage> result);

        UniTaskVoid HandleRequestSwapOrMergeStorageItem(
            RequestHandlerData requestHandler, RequestSwapOrMergeStorageItemMessage request,
            RequestProceedResultDelegate<ResponseSwapOrMergeStorageItemMessage> result);
    }
}
