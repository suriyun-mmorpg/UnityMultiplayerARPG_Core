using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientStorageHandlers
    {
        bool RequestOpenStorage(RequestOpenStorageMessage data, ResponseDelegate<ResponseOpenStorageMessage> callback);
        bool RequestCloseStorage(ResponseDelegate<ResponseCloseStorageMessage> callback);
        bool RequestMoveItemFromStorage(RequestMoveItemFromStorageMessage data, ResponseDelegate<ResponseMoveItemFromStorageMessage> callback);
        bool RequestMoveItemToStorage(RequestMoveItemToStorageMessage data, ResponseDelegate<ResponseMoveItemToStorageMessage> callback);
        bool RequestSwapOrMergeStorageItem(RequestSwapOrMergeStorageItemMessage data, ResponseDelegate<ResponseSwapOrMergeStorageItemMessage> callback);
    }
}
