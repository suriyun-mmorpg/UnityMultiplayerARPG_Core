using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class UIStorageResponses
    {
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
        }
    }
}
