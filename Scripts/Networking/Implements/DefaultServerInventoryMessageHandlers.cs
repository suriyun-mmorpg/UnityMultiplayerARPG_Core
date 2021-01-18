using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultServerInventoryMessageHandlers : MonoBehaviour, IServerInventoryMessageHandlers
    {
        public async UniTaskVoid HandleRequestSwapOrMergeItem(RequestHandlerData requestHandler, RequestSwapOrMergeItemMessage request, RequestProceedResultDelegate<ResponseSwapOrMergeItemMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeItemMessage()
                {
                    error = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.SwapOrMergeItem(request.fromIndex, request.toIndex, out gameMessage))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, gameMessage);
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeItemMessage()
                {
                    error = gameMessage,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseSwapOrMergeItemMessage());
        }

        public async UniTaskVoid HandleRequestEquipArmor(RequestHandlerData requestHandler, RequestEquipArmorMessage request, RequestProceedResultDelegate<ResponseEquipArmorMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseEquipArmorMessage()
                {
                    error = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.EquipArmor(request.nonEquipIndex, request.equipSlotIndex, out gameMessage))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, gameMessage);
                result.Invoke(AckResponseCode.Error, new ResponseEquipArmorMessage()
                {
                    error = gameMessage,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseEquipArmorMessage());
        }

        public async UniTaskVoid HandleRequestEquipWeapon(RequestHandlerData requestHandler, RequestEquipWeaponMessage request, RequestProceedResultDelegate<ResponseEquipWeaponMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseEquipWeaponMessage()
                {
                    error = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.EquipWeapon(request.nonEquipIndex, request.equipWeaponSet, request.isLeftHand, out gameMessage))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, gameMessage);
                result.Invoke(AckResponseCode.Error, new ResponseEquipWeaponMessage()
                {
                    error = gameMessage,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseEquipWeaponMessage());
        }

        public async UniTaskVoid HandleRequestUnEquipArmor(RequestHandlerData requestHandler, RequestUnEquipArmorMessage request, RequestProceedResultDelegate<ResponseUnEquipArmorMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseUnEquipArmorMessage()
                {
                    error = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.UnEquipArmor(request.equipIndex, false, out gameMessage, out _, request.nonEquipIndex))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, gameMessage);
                result.Invoke(AckResponseCode.Error, new ResponseUnEquipArmorMessage()
                {
                    error = gameMessage,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseUnEquipArmorMessage());
        }

        public async UniTaskVoid HandleRequestUnEquipWeapon(RequestHandlerData requestHandler, RequestUnEquipWeaponMessage request, RequestProceedResultDelegate<ResponseUnEquipWeaponMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseUnEquipWeaponMessage()
                {
                    error = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.UnEquipWeapon(request.equipWeaponSet, request.isLeftHand, false, out gameMessage, out _, request.nonEquipIndex))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, gameMessage);
                result.Invoke(AckResponseCode.Error, new ResponseUnEquipWeaponMessage()
                {
                    error = gameMessage,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseUnEquipWeaponMessage());
        }

        public async UniTaskVoid HandleRequestSwitchEquipWeaponSet(RequestHandlerData requestHandler, RequestSwitchEquipWeaponSetMessage request, RequestProceedResultDelegate<ResponseSwitchEquipWeaponSetMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSwitchEquipWeaponSetMessage()
                {
                    error = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            byte equipWeaponSet = request.equipWeaponSet;
            if (equipWeaponSet >= GameInstance.Singleton.maxEquipWeaponSet)
                equipWeaponSet = (byte)(GameInstance.Singleton.maxEquipWeaponSet - 1);
            playerCharacter.FillWeaponSetsIfNeeded(equipWeaponSet);
            playerCharacter.EquipWeaponSet = equipWeaponSet;
            result.Invoke(AckResponseCode.Success, new ResponseSwitchEquipWeaponSetMessage());
        }
    }
}
