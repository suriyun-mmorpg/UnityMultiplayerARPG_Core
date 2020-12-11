using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientStorageHandlers
    {
        bool RequestGetStorageItems(
            string characterId,
            StorageType storageType,
            string storageOwnerId,
            ResponseDelegate<ResponseGetStorageItemsMessage> callback);
        bool RequestMoveItemFromStorage(
            string characterId,
            StorageType storageType,
            string storageOwnerId,
            short storageItemIndex,
            short amount,
            short inventoryIndex,
            ResponseDelegate<ResponseMoveItemFromStorageMessage> callback);
        bool RequestMoveItemToStorage(
            string characterId,
            StorageType storageType,
            string storageOwnerId,
            short inventoryIndex,
            short amount,
            short storageItemIndex,
            ResponseDelegate<ResponseMoveItemToStorageMessage> callback);
        bool RequestSwapOrMergeStorageItem(
            string characterId,
            StorageType storageType,
            string storageOwnerId,
            short fromIndex,
            short toIndex,
            ResponseDelegate<ResponseSwapOrMergeStorageItemMessage> callback);
    }
}
