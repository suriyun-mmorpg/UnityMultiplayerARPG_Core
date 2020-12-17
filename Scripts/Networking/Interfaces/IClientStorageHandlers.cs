using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientStorageHandlers
    {
        StorageType StorageType { get; set; }
        string StorageOwnerId { get; set; }
        bool RequestMoveItemFromStorage(RequestMoveItemFromStorageMessage data, ResponseDelegate<ResponseMoveItemFromStorageMessage> callback);
        bool RequestMoveItemToStorage(RequestMoveItemToStorageMessage data, ResponseDelegate<ResponseMoveItemToStorageMessage> callback);
        bool RequestSwapOrMergeStorageItem(RequestSwapOrMergeStorageItemMessage data, ResponseDelegate<ResponseSwapOrMergeStorageItemMessage> callback);
    }
}
