using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IServerInventoryMessageHandlers
    {
        IServerPlayerCharacterHandlers ServerPlayerCharacterHandlers { get; set; }

        UniTaskVoid HandleRequestSwapOrMergeItem(
            RequestHandlerData requestHandler, RequestSwapOrMergeItemMessage request,
            RequestProceedResultDelegate<ResponseSwapOrMergeItemMessage> result);

        UniTaskVoid HandleRequestEquipWeapon(
            RequestHandlerData requestHandler, RequestEquipWeaponMessage request,
            RequestProceedResultDelegate<ResponseEquipWeaponMessage> result);

        UniTaskVoid HandleRequestEquipArmor(
            RequestHandlerData requestHandler, RequestEquipArmorMessage request,
            RequestProceedResultDelegate<ResponseEquipArmorMessage> result);

        UniTaskVoid HandleRequestUnEquipWeapon(
            RequestHandlerData requestHandler, RequestUnEquipWeaponMessage request,
            RequestProceedResultDelegate<ResponseUnEquipWeaponMessage> result);

        UniTaskVoid HandleRequestUnEquipArmor(
            RequestHandlerData requestHandler, RequestUnEquipArmorMessage request,
            RequestProceedResultDelegate<ResponseUnEquipArmorMessage> result);

    }
}
