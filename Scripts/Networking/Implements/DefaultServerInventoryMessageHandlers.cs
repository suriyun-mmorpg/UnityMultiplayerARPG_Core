using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultServerInventoryMessageHandlers : MonoBehaviour, IServerInventoryMessageHandlers
    {
        public async UniTaskVoid HandleRequestSwapOrMergeItem(RequestHandlerData requestHandler, RequestSwapOrMergeItemMessage request, RequestProceedResultDelegate<ResponseSwapOrMergeItemMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp <= 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeItemMessage());
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.SwapOrMergeItem(request.fromIndex, request.toIndex, out gameMessage))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeItemMessage()
                {
                    message = gameMessage,
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
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp <= 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseEquipArmorMessage());
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.EquipArmor(request.nonEquipIndex, request.equipSlotIndex, out gameMessage))
            {
                result.Invoke(AckResponseCode.Error, new ResponseEquipArmorMessage()
                {
                    message = gameMessage,
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
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp <= 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseEquipWeaponMessage());
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.EquipWeapon(request.nonEquipIndex, request.equipWeaponSet, request.isLeftHand, out gameMessage))
            {
                result.Invoke(AckResponseCode.Error, new ResponseEquipWeaponMessage()
                {
                    message = gameMessage,
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
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp <= 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseUnEquipArmorMessage());
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.UnEquipArmor(request.equipIndex, false, out gameMessage, out _, request.nonEquipIndex))
            {
                result.Invoke(AckResponseCode.Error, new ResponseUnEquipArmorMessage()
                {
                    message = gameMessage,
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
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp <= 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseUnEquipWeaponMessage());
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.UnEquipWeapon(request.equipWeaponSet, request.isLeftHand, false, out gameMessage, out _, request.nonEquipIndex))
            {
                result.Invoke(AckResponseCode.Error, new ResponseUnEquipWeaponMessage()
                {
                    message = gameMessage,
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
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null)
            {
                if (!playerCharacterEntity.CanDoActions())
                {
                    result.Invoke(AckResponseCode.Error, new ResponseSwitchEquipWeaponSetMessage());
                    return;
                }
            }

            byte equipWeaponSet = request.equipWeaponSet;
            if (equipWeaponSet >= GameInstance.Singleton.maxEquipWeaponSet)
                equipWeaponSet = 0;
            playerCharacter.FillWeaponSetsIfNeeded(equipWeaponSet);
            playerCharacter.EquipWeaponSet = equipWeaponSet;

            result.Invoke(AckResponseCode.Success, new ResponseSwitchEquipWeaponSetMessage());
        }

        public async UniTaskVoid HandleRequestDismantleItem(RequestHandlerData requestHandler, RequestDismantleItemMessage request, RequestProceedResultDelegate<ResponseDismantleItemMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseDismantleItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp <= 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseDismantleItemMessage());
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.DismantleItem(request.index, request.amount, out gameMessage))
            {
                result.Invoke(AckResponseCode.Error, new ResponseDismantleItemMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseDismantleItemMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestDismantleItems(RequestHandlerData requestHandler, RequestDismantleItemsMessage request, RequestProceedResultDelegate<ResponseDismantleItemsMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseDismantleItemsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp <= 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseDismantleItemsMessage());
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.DismantleItems(request.selectedIndexes, out gameMessage))
            {
                result.Invoke(AckResponseCode.Error, new ResponseDismantleItemsMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseDismantleItemsMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestEnhanceSocketItem(RequestHandlerData requestHandler, RequestEnhanceSocketItemMessage request, RequestProceedResultDelegate<ResponseEnhanceSocketItemMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseEnhanceSocketItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp <= 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseEnhanceSocketItemMessage());
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.EnhanceSocketItem(request.inventoryType, request.index, request.enhancerId, request.socketIndex, out gameMessage))
            {
                result.Invoke(AckResponseCode.Error, new ResponseEnhanceSocketItemMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseEnhanceSocketItemMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestRefineItem(RequestHandlerData requestHandler, RequestRefineItemMessage request, RequestProceedResultDelegate<ResponseRefineItemMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseRefineItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp <= 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseRefineItemMessage());
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.RefineItem(request.inventoryType, request.index, out gameMessage))
            {
                result.Invoke(AckResponseCode.Error, new ResponseRefineItemMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseRefineItemMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestRemoveEnhancerFromItem(RequestHandlerData requestHandler, RequestRemoveEnhancerFromItemMessage request, RequestProceedResultDelegate<ResponseRemoveEnhancerFromItemMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseRemoveEnhancerFromItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp <= 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseRemoveEnhancerFromItemMessage());
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.RemoveEnhancerFromItem(request.inventoryType, request.index, request.socketIndex, out gameMessage))
            {
                result.Invoke(AckResponseCode.Error, new ResponseRemoveEnhancerFromItemMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseRemoveEnhancerFromItemMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestRepairItem(RequestHandlerData requestHandler, RequestRepairItemMessage request, RequestProceedResultDelegate<ResponseRepairItemMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseRepairItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp <= 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseRepairItemMessage());
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.RepairItem(request.inventoryType, request.index, out gameMessage))
            {
                result.Invoke(AckResponseCode.Error, new ResponseRepairItemMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseRepairItemMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestRepairEquipItems(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseRepairEquipItemsMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseRepairEquipItemsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp <= 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseRepairEquipItemsMessage());
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.RepairEquipItems(out gameMessage))
            {
                result.Invoke(AckResponseCode.Error, new ResponseRepairEquipItemsMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseRepairEquipItemsMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestSellItem(RequestHandlerData requestHandler, RequestSellItemMessage request, RequestProceedResultDelegate<ResponseSellItemMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSellItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp <= 0 || !playerCharacter.AccessingNpcShopDialog(out _))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSellItemMessage());
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.SellItem(request.index, request.amount, out gameMessage))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSellItemMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseSellItemMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestSellItems(RequestHandlerData requestHandler, RequestSellItemsMessage request, RequestProceedResultDelegate<ResponseSellItemsMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSellItemsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp <= 0 || !playerCharacter.AccessingNpcShopDialog(out _))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSellItemsMessage());
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.SellItems(request.selectedIndexes, out gameMessage))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSellItemsMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseSellItemsMessage()
            {
                message = gameMessage,
            });
        }
    }
}
