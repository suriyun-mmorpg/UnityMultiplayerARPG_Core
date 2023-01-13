using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class ClientStorageActions
    {
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseOpenStorageMessage> onResponseOpenStorage;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseCloseStorageMessage> onResponseCloseStorage;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseMoveItemFromStorageMessage> onResponseMoveItemFromStorage;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseMoveItemToStorageMessage> onResponseMoveItemToStorage;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseSwapOrMergeStorageItemMessage> onResponseSwapOrMergeStorageItem;
        public static System.Action<StorageType, string, uint, int, int> onNotifyStorageOpened;
        public static System.Action onNotifyStorageClosed;
        public static System.Action<List<CharacterItem>> onNotifyStorageItemsUpdated;

        public static void ResponseOpenStorage(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseOpenStorageMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseOpenStorage != null)
                onResponseOpenStorage.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseCloseStorage(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCloseStorageMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseCloseStorage != null)
                onResponseCloseStorage.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseMoveItemFromStorage(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseMoveItemFromStorageMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseMoveItemFromStorage != null)
                onResponseMoveItemFromStorage.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseMoveItemToStorage(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseMoveItemToStorageMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseMoveItemToStorage != null)
                onResponseMoveItemToStorage.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseSwapOrMergeStorageItem(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSwapOrMergeStorageItemMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseSwapOrMergeStorageItem != null)
                onResponseSwapOrMergeStorageItem.Invoke(requestHandler, responseCode, response);
        }

        public static void NotifyStorageOpened(StorageType storageType, string storageOwnerId, uint objectId, int weightLimit, int slotLimit)
        {
            GameInstance.OpenedStorageType = storageType;
            GameInstance.OpenedStorageOwnerId = storageOwnerId;
            GameInstance.ItemUIVisibilityManager.ShowStorageDialog(storageType, storageOwnerId, objectId, weightLimit, slotLimit);
            if (onNotifyStorageOpened != null)
                onNotifyStorageOpened.Invoke(storageType, storageOwnerId, objectId, weightLimit, slotLimit);
        }

        public static void NotifyStorageClosed()
        {
            GameInstance.OpenedStorageType = StorageType.None;
            GameInstance.OpenedStorageOwnerId = string.Empty;
            GameInstance.ItemUIVisibilityManager.HideStorageDialog();
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
