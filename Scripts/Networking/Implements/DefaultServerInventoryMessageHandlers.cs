using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultServerInventoryMessageHandlers : MonoBehaviour, IServerInventoryMessageHandlers
    {
        public IServerPlayerCharacterHandlers ServerPlayerCharacterHandlers { get; set; }

        public async UniTaskVoid HandleRequestSwapOrMergeItem(RequestHandlerData requestHandler, RequestSwapOrMergeItemMessage request, RequestProceedResultDelegate<ResponseSwapOrMergeItemMessage> result)
        {
            await UniTask.Yield();
            PlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeItemMessage()
                {
                    error = ResponseSwapOrMergeItemMessage.Error.CharacterNotFound,
                });
                return;
            }
            GameMessage.Type gameMessage;
            if (!playerCharacter.SwapOrMergeItem(request.fromIndex, request.toIndex, out gameMessage))
            {
                ResponseSwapOrMergeItemMessage.Error error = ResponseSwapOrMergeItemMessage.Error.NotAllowed;
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, gameMessage);
                switch(gameMessage)
                {
                    case GameMessage.Type.InvalidItemData:
                        error = ResponseSwapOrMergeItemMessage.Error.InvalidItemIndex;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeItemMessage()
                {
                    error = error,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseSwapOrMergeItemMessage());
        }

        public async UniTaskVoid HandleRequestEquipArmor(RequestHandlerData requestHandler, RequestEquipArmorMessage request, RequestProceedResultDelegate<ResponseEquipArmorMessage> result)
        {
            await UniTask.Yield();
            PlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseEquipArmorMessage()
                {
                    error = ResponseEquipArmorMessage.Error.CharacterNotFound,
                });
                return;
            }
            GameMessage.Type gameMessage;
            if (!playerCharacter.EquipArmor(request.nonEquipIndex, request.equipSlotIndex, out gameMessage))
            {
                ResponseEquipArmorMessage.Error error = ResponseEquipArmorMessage.Error.NotAllowed;
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, gameMessage);
                switch (gameMessage)
                {
                    case GameMessage.Type.InvalidItemData:
                        error = ResponseEquipArmorMessage.Error.InvalidItemIndex;
                        break;
                    case GameMessage.Type.CannotCarryAnymore:
                        error = ResponseEquipArmorMessage.Error.CannotCarryAllItems;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseEquipArmorMessage()
                {
                    error = error,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseEquipArmorMessage());
        }

        public async UniTaskVoid HandleRequestEquipWeapon(RequestHandlerData requestHandler, RequestEquipWeaponMessage request, RequestProceedResultDelegate<ResponseEquipWeaponMessage> result)
        {
            await UniTask.Yield();
            PlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseEquipWeaponMessage()
                {
                    error = ResponseEquipWeaponMessage.Error.CharacterNotFound,
                });
                return;
            }
            GameMessage.Type gameMessage;
            if (!playerCharacter.EquipWeapon(request.nonEquipIndex, request.equipWeaponSet, request.isLeftHand, out gameMessage))
            {
                ResponseEquipWeaponMessage.Error error = ResponseEquipWeaponMessage.Error.NotAllowed;
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, gameMessage);
                switch (gameMessage)
                {
                    case GameMessage.Type.InvalidItemData:
                        error = ResponseEquipWeaponMessage.Error.InvalidItemIndex;
                        break;
                    case GameMessage.Type.CannotCarryAnymore:
                        error = ResponseEquipWeaponMessage.Error.CannotCarryAllItems;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseEquipWeaponMessage()
                {
                    error = error,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseEquipWeaponMessage());
        }

        public async UniTaskVoid HandleRequestUnEquipArmor(RequestHandlerData requestHandler, RequestUnEquipArmorMessage request, RequestProceedResultDelegate<ResponseUnEquipArmorMessage> result)
        {
            await UniTask.Yield();
            PlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseUnEquipArmorMessage()
                {
                    error = ResponseUnEquipArmorMessage.Error.CharacterNotFound,
                });
                return;
            }
            GameMessage.Type gameMessage;
            if (!playerCharacter.UnEquipArmor(request.equipIndex, false, out gameMessage, out _, request.nonEquipIndex))
            {
                ResponseUnEquipArmorMessage.Error error = ResponseUnEquipArmorMessage.Error.NotAllowed;
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, gameMessage);
                switch (gameMessage)
                {
                    case GameMessage.Type.InvalidItemData:
                        error = ResponseUnEquipArmorMessage.Error.InvalidItemIndex;
                        break;
                    case GameMessage.Type.CannotCarryAnymore:
                        error = ResponseUnEquipArmorMessage.Error.CannotCarryAllItems;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseUnEquipArmorMessage()
                {
                    error = error,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseUnEquipArmorMessage());
        }

        public async UniTaskVoid HandleRequestUnEquipWeapon(RequestHandlerData requestHandler, RequestUnEquipWeaponMessage request, RequestProceedResultDelegate<ResponseUnEquipWeaponMessage> result)
        {
            await UniTask.Yield();
            PlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseUnEquipWeaponMessage()
                {
                    error = ResponseUnEquipWeaponMessage.Error.CharacterNotFound,
                });
                return;
            }
            GameMessage.Type gameMessage;
            if (!playerCharacter.UnEquipWeapon(request.equipWeaponSet, request.isLeftHand, false, out gameMessage, out _, request.nonEquipIndex))
            {
                ResponseUnEquipWeaponMessage.Error error = ResponseUnEquipWeaponMessage.Error.NotAllowed;
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, gameMessage);
                switch (gameMessage)
                {
                    case GameMessage.Type.InvalidItemData:
                        error = ResponseUnEquipWeaponMessage.Error.InvalidItemIndex;
                        break;
                    case GameMessage.Type.CannotCarryAnymore:
                        error = ResponseUnEquipWeaponMessage.Error.CannotCarryAllItems;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseUnEquipWeaponMessage()
                {
                    error = error,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseUnEquipWeaponMessage());
        }
    }
}
