using LiteNetLibManager;
using UnityEngine;
using ReqTypes = MultiplayerARPG.BaseGameNetworkManager.ReqTypes;

namespace MultiplayerARPG
{
    public class DefaultClientInventoryHandlers : MonoBehaviour, IClientInventoryHandlers
    {
        public bool RequestSwapOrMergeItem(RequestSwapOrMergeItemMessage data, ResponseDelegate<ResponseSwapOrMergeItemMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.SwapOrMergeItem, data, responseDelegate: callback);
        }

        public bool RequestEquipWeapon(RequestEquipWeaponMessage data, ResponseDelegate<ResponseEquipWeaponMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.EquipWeapon, data, responseDelegate: callback);
        }

        public bool RequestEquipArmor(RequestEquipArmorMessage data, ResponseDelegate<ResponseEquipArmorMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.EquipArmor, data, responseDelegate: callback);
        }

        public bool RequestUnEquipWeapon(RequestUnEquipWeaponMessage data, ResponseDelegate<ResponseUnEquipWeaponMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.UnEquipWeapon, data, responseDelegate: callback);
        }

        public bool RequestUnEquipArmor(RequestUnEquipArmorMessage data, ResponseDelegate<ResponseUnEquipArmorMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.UnEquipArmor, data, responseDelegate: callback);
        }
    }
}
