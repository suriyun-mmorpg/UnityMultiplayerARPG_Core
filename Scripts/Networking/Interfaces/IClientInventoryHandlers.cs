using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientInventoryHandlers
    {
        bool RequestSwapOrMergeItem(RequestSwapOrMergeItemMessage data, ResponseDelegate<ResponseSwapOrMergeItemMessage> callback);
        bool RequestEquipWeapon(RequestEquipWeaponMessage data, ResponseDelegate<ResponseEquipWeaponMessage> callback);
        bool RequestEquipArmor(RequestEquipArmorMessage data, ResponseDelegate<ResponseEquipArmorMessage> callback);
        bool RequestUnEquipWeapon(RequestUnEquipWeaponMessage data, ResponseDelegate<ResponseUnEquipWeaponMessage> callback);
        bool RequestUnEquipArmor(RequestUnEquipArmorMessage data, ResponseDelegate<ResponseUnEquipArmorMessage> callback);
    }
}
