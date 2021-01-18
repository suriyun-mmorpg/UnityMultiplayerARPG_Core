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

        public static async UniTaskVoid ResponseSwapOrMergeItem(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSwapOrMergeItemMessage response)
        {
            await UniTask.Yield();
            if (onResponseSwapOrMergeItem != null)
                onResponseSwapOrMergeItem.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseEquipArmor(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseEquipArmorMessage response)
        {
            await UniTask.Yield();
            if (onResponseEquipArmor != null)
                onResponseEquipArmor.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseEquipWeapon(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseEquipWeaponMessage response)
        {
            await UniTask.Yield();
            if (onResponseEquipWeapon != null)
                onResponseEquipWeapon.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseUnEquipArmor(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseUnEquipArmorMessage response)
        {
            await UniTask.Yield();
            if (onResponseUnEquipArmor != null)
                onResponseUnEquipArmor.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseUnEquipWeapon(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseUnEquipWeaponMessage response)
        {
            await UniTask.Yield();
            if (onResponseUnEquipWeapon != null)
                onResponseUnEquipWeapon.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseSwitchEquipWeaponSet(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSwitchEquipWeaponSetMessage response)
        {
            await UniTask.Yield();
            if (onResponseSwitchEquipWeaponSet != null)
                onResponseSwitchEquipWeaponSet.Invoke(requestHandler, responseCode, response);
        }
    }
}
