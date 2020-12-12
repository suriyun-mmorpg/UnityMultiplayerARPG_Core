using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientStorageHandlers
    {
        bool RequestGetStorageItems(RequestGetStorageItemsMessage data, ResponseDelegate<ResponseGetStorageItemsMessage> callback);
        bool RequestMoveItemFromStorage(RequestMoveItemFromStorageMessage data, ResponseDelegate<ResponseMoveItemFromStorageMessage> callback);
        bool RequestMoveItemToStorage(RequestMoveItemToStorageMessage data, ResponseDelegate<ResponseMoveItemToStorageMessage> callback);
        bool RequestSwapOrMergeStorageItem(RequestSwapOrMergeStorageItemMessage data, ResponseDelegate<ResponseSwapOrMergeStorageItemMessage> callback);
    }
}
