using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientInventoryHandlers : MonoBehaviour, IClientInventoryHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public bool RequestSwapOrMergeItem(RequestSwapOrMergeItemMessage data, ResponseDelegate<ResponseSwapOrMergeItemMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.SwapOrMergeItem, data, responseDelegate: callback);
        }

        public bool RequestEquipWeapon(RequestEquipWeaponMessage data, ResponseDelegate<ResponseEquipWeaponMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.EquipWeapon, data, responseDelegate: callback);
        }

        public bool RequestEquipArmor(RequestEquipArmorMessage data, ResponseDelegate<ResponseEquipArmorMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.EquipArmor, data, responseDelegate: callback);
        }

        public bool RequestUnEquipWeapon(RequestUnEquipWeaponMessage data, ResponseDelegate<ResponseUnEquipWeaponMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.UnEquipWeapon, data, responseDelegate: callback);
        }

        public bool RequestUnEquipArmor(RequestUnEquipArmorMessage data, ResponseDelegate<ResponseUnEquipArmorMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.UnEquipArmor, data, responseDelegate: callback);
        }
    }
}
