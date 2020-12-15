using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class ClientStorageActions
    {
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseMoveItemFromStorageMessage> onResponseMoveItemFromStorage;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseMoveItemToStorageMessage> onResponseMoveItemToStorage;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseSwapOrMergeStorageItemMessage> onResponseSwapOrMergeStorageItem;

        public static async UniTaskVoid ResponseMoveItemFromStorage(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseMoveItemFromStorageMessage response)
        {
            await UniTask.Yield();
            if (responseCode == AckResponseCode.Success)
            {
                UIStorageItems[] uis = Object.FindObjectsOfType<UIStorageItems>();
                foreach (UIStorageItems ui in uis)
                {
                    ui.Refresh();
                }
            }
            if (onResponseMoveItemFromStorage != null)
                onResponseMoveItemFromStorage.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseMoveItemToStorage(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseMoveItemToStorageMessage response)
        {
            await UniTask.Yield();
            if (responseCode == AckResponseCode.Success)
            {
                UIStorageItems[] uis = Object.FindObjectsOfType<UIStorageItems>();
                foreach (UIStorageItems ui in uis)
                {
                    ui.Refresh();
                }
            }
            if (onResponseMoveItemToStorage != null)
                onResponseMoveItemToStorage.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseSwapOrMergeStorageItem(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSwapOrMergeStorageItemMessage response)
        {
            await UniTask.Yield();
            if (responseCode == AckResponseCode.Success)
            {
                UIStorageItems[] uis = Object.FindObjectsOfType<UIStorageItems>();
                foreach (UIStorageItems ui in uis)
                {
                    ui.Refresh();
                }
            }
            if (onResponseSwapOrMergeStorageItem != null)
                onResponseSwapOrMergeStorageItem.Invoke(requestHandler, responseCode, response);
        }
    }
}
