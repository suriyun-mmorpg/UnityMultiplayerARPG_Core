using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static class ClientInventoryActions
    {
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseSwapOrMergeItemMessage> onResponseSwapOrMergeItem;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseEquipArmorMessage> onResponseEquipArmor;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseEquipWeaponMessage> onResponseEquipWeapon;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseUnEquipArmorMessage> onResponseUnEquipArmor;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseUnEquipWeaponMessage> onResponseUnEquipWeapon;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseSwitchEquipWeaponSetMessage> onResponseSwitchEquipWeaponSet;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseDismantleItemMessage> onResponseDismantleItem;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseDismantleItemsMessage> onResponseDismantleItems;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseEnhanceSocketItemMessage> onResponseEnhanceSocketItem;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseRefineItemMessage> onResponseRefineItem;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseRemoveEnhancerFromItemMessage> onResponseRemoveEnhancerFromItem;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseRepairItemMessage> onResponseRepairItem;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseRepairEquipItemsMessage> onResponseRepairEquipItems;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseSellItemMessage> onResponseSellItem;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseSellItemsMessage> onResponseSellItems;

        public static async UniTaskVoid ResponseSwapOrMergeItem(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSwapOrMergeItemMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseSwapOrMergeItem != null)
                onResponseSwapOrMergeItem.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseEquipArmor(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseEquipArmorMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseEquipArmor != null)
                onResponseEquipArmor.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseEquipWeapon(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseEquipWeaponMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseEquipWeapon != null)
                onResponseEquipWeapon.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseUnEquipArmor(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseUnEquipArmorMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseUnEquipArmor != null)
                onResponseUnEquipArmor.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseUnEquipWeapon(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseUnEquipWeaponMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseUnEquipWeapon != null)
                onResponseUnEquipWeapon.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseSwitchEquipWeaponSet(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSwitchEquipWeaponSetMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseSwitchEquipWeaponSet != null)
                onResponseSwitchEquipWeaponSet.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseDismantleItem(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseDismantleItemMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseDismantleItem != null)
                onResponseDismantleItem.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseDismantleItems(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseDismantleItemsMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseDismantleItems != null)
                onResponseDismantleItems.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseEnhanceSocketItem(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseEnhanceSocketItemMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseEnhanceSocketItem != null)
                onResponseEnhanceSocketItem.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseRefineItem(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseRefineItemMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseRefineItem != null)
                onResponseRefineItem.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseRemoveEnhancerFromItem(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseRemoveEnhancerFromItemMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseRemoveEnhancerFromItem != null)
                onResponseRemoveEnhancerFromItem.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseRepairItem(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseRepairItemMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseRepairItem != null)
                onResponseRepairItem.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseRepairEquipItems(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseRepairEquipItemsMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseRepairEquipItems != null)
                onResponseRepairEquipItems.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseSellItem(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSellItemMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseSellItem != null)
                onResponseSellItem.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseSellItems(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSellItemsMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseSellItems != null)
                onResponseSellItems.Invoke(requestHandler, responseCode, response);
        }
    }
}
