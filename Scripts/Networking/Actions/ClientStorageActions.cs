using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class ClientStorageActions
    {
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseMoveItemFromStorageMessage> onResponseMoveItemFromStorage;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseMoveItemToStorageMessage> onResponseMoveItemToStorage;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseSwapOrMergeStorageItemMessage> onResponseSwapOrMergeStorageItem;
        public static System.Action<StorageType, string, uint, short, short> onNotifyStorageOpened;
        public static System.Action onNotifyStorageClosed;
        public static System.Action<List<CharacterItem>> onNotifyStorageItemsUpdated;

        public static async UniTaskVoid ResponseMoveItemFromStorage(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseMoveItemFromStorageMessage response)
        {
            await UniTask.Yield();
            if (onResponseMoveItemFromStorage != null)
                onResponseMoveItemFromStorage.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseMoveItemToStorage(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseMoveItemToStorageMessage response)
        {
            await UniTask.Yield();
            if (onResponseMoveItemToStorage != null)
                onResponseMoveItemToStorage.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseSwapOrMergeStorageItem(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSwapOrMergeStorageItemMessage response)
        {
            await UniTask.Yield();
            if (onResponseSwapOrMergeStorageItem != null)
                onResponseSwapOrMergeStorageItem.Invoke(requestHandler, responseCode, response);
        }

        public static void NotifyStorageOpened(StorageType storageType, string storageOwnerId, uint objectId, short weightLimit, short slotLimit)
        {
            GameInstance.OpenedStorageType = storageType;
            GameInstance.OpenedStorageOwnerId = storageOwnerId;
            if (onNotifyStorageOpened != null)
                onNotifyStorageOpened.Invoke(storageType, storageOwnerId, objectId, weightLimit, slotLimit);
        }

        public static void NotifyStorageClosed()
        {
            GameInstance.OpenedStorageType = StorageType.None;
            GameInstance.OpenedStorageOwnerId = string.Empty;
            if (onNotifyStorageClosed != null)
                onNotifyStorageClosed.Invoke();
        }

        public static void NotifyStorageItemsUpdated(List<CharacterItem> storageItems)
        {
            if (onNotifyStorageItemsUpdated != null)
                onNotifyStorageItemsUpdated.Invoke(storageItems);
        }
    }
}
