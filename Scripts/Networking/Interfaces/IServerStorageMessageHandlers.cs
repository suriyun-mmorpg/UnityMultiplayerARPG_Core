using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    /// <summary>
    /// These properties and functions will be called at server only
    /// </summary>
    public interface IServerStorageMessageHandlers
    {
        IServerPlayerCharacterHandlers ServerPlayerCharacterHandlers { get; set; }
        IServerStorageHandlers ServerStorageHandlers { get; set; }

        UniTaskVoid HandleRequestGetStorageItems(
            RequestHandlerData requestHandler, RequestGetStorageItemsMessage request,
            RequestProceedResultDelegate<ResponseGetStorageItemsMessage> result);

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
