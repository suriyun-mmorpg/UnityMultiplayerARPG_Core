using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IInventoryMessageHandlers
    {
        IServerPlayerCharacterHandlers ServerPlayerCharacterHandlers { get; set; }

        UniTaskVoid HandleRequestSwapOrMergeItemMessage(
            RequestHandlerData requestHandler, RequestSwapOrMergeItemMessage request,
            RequestProceedResultDelegate<ResponseSwapOrMergeItemMessage> result);

        UniTaskVoid HandleRequestEquipWeaponMessage(
            RequestHandlerData requestHandler, RequestEquipWeaponMessage request,
            RequestProceedResultDelegate<ResponseEquipWeaponMessage> result);

        UniTaskVoid HandleRequestEquipArmorMessage(
            RequestHandlerData requestHandler, RequestEquipArmorMessage request,
            RequestProceedResultDelegate<ResponseEquipArmorMessage> result);

        UniTaskVoid HandleRequestUnEquipWeaponMessage(
            RequestHandlerData requestHandler, RequestUnEquipWeaponMessage request,
            RequestProceedResultDelegate<ResponseUnEquipWeaponMessage> result);

        UniTaskVoid HandleRequestUnEquipArmorMessage(
            RequestHandlerData requestHandler, RequestUnEquipArmorMessage request,
            RequestProceedResultDelegate<ResponseUnEquipArmorMessage> result);

    }
}
