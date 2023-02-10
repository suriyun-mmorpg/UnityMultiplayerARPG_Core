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
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseSwapOrMergeItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanMoveItem())
            {
                result.InvokeError(new ResponseSwapOrMergeItemMessage());
                return;
            }

            if (!playerCharacter.SwapOrMergeItem(request.fromIndex, request.toIndex, out UITextKeys gameMessage))
            {
                result.InvokeError(new ResponseSwapOrMergeItemMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.InvokeSuccess(new ResponseSwapOrMergeItemMessage());
        }

        public async UniTaskVoid HandleRequestEquipArmor(RequestHandlerData requestHandler, RequestEquipArmorMessage request, RequestProceedResultDelegate<ResponseEquipArmorMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseEquipArmorMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanEquipItem())
            {
                result.InvokeError(new ResponseEquipArmorMessage());
                return;
            }

            if (!playerCharacter.EquipArmor(request.nonEquipIndex, request.equipSlotIndex, out UITextKeys gameMessage))
            {
                result.InvokeError(new ResponseEquipArmorMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.InvokeSuccess(new ResponseEquipArmorMessage());
        }

        public async UniTaskVoid HandleRequestEquipWeapon(RequestHandlerData requestHandler, RequestEquipWeaponMessage request, RequestProceedResultDelegate<ResponseEquipWeaponMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseEquipWeaponMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanEquipItem())
            {
                result.InvokeError(new ResponseEquipWeaponMessage());
                return;
            }

            if (!playerCharacter.EquipWeapon(request.nonEquipIndex, request.equipWeaponSet, request.isLeftHand, out UITextKeys gameMessage))
            {
                result.InvokeError(new ResponseEquipWeaponMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.InvokeSuccess(new ResponseEquipWeaponMessage());
        }

        public async UniTaskVoid HandleRequestUnEquipArmor(RequestHandlerData requestHandler, RequestUnEquipArmorMessage request, RequestProceedResultDelegate<ResponseUnEquipArmorMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseUnEquipArmorMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanUnEquipItem())
            {
                result.InvokeError(new ResponseUnEquipArmorMessage());
                return;
            }

            if (!playerCharacter.UnEquipArmor(request.equipIndex, false, out UITextKeys gameMessage, out _, request.nonEquipIndex))
            {
                result.InvokeError(new ResponseUnEquipArmorMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.InvokeSuccess(new ResponseUnEquipArmorMessage());
        }

        public async UniTaskVoid HandleRequestUnEquipWeapon(RequestHandlerData requestHandler, RequestUnEquipWeaponMessage request, RequestProceedResultDelegate<ResponseUnEquipWeaponMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseUnEquipWeaponMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanUnEquipItem())
            {
                result.InvokeError(new ResponseUnEquipWeaponMessage());
                return;
            }

            if (!playerCharacter.UnEquipWeapon(request.equipWeaponSet, request.isLeftHand, false, out UITextKeys gameMessage, out _, request.nonEquipIndex))
            {
                result.InvokeError(new ResponseUnEquipWeaponMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.InvokeSuccess(new ResponseUnEquipWeaponMessage());
        }

        public async UniTaskVoid HandleRequestSwitchEquipWeaponSet(RequestHandlerData requestHandler, RequestSwitchEquipWeaponSetMessage request, RequestProceedResultDelegate<ResponseSwitchEquipWeaponSetMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseSwitchEquipWeaponSetMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanEquipItem())
            {
                result.InvokeError(new ResponseSwitchEquipWeaponSetMessage());
                return;
            }

            byte equipWeaponSet = request.equipWeaponSet;
            if (equipWeaponSet >= GameInstance.Singleton.maxEquipWeaponSet)
                equipWeaponSet = 0;
            playerCharacter.FillWeaponSetsIfNeeded(equipWeaponSet);
            playerCharacter.EquipWeaponSet = equipWeaponSet;

            result.InvokeSuccess(new ResponseSwitchEquipWeaponSetMessage());
        }

        public async UniTaskVoid HandleRequestDismantleItem(RequestHandlerData requestHandler, RequestDismantleItemMessage request, RequestProceedResultDelegate<ResponseDismantleItemMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseDismantleItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanDismentleItem())
            {
                result.InvokeError(new ResponseDismantleItemMessage());
                return;
            }

            if (!playerCharacter.DismantleItem(request.index, request.amount, out UITextKeys gameMessage))
            {
                result.InvokeError(new ResponseDismantleItemMessage()
                {
                    message = gameMessage,
                });
                return;
            }

            result.InvokeSuccess(new ResponseDismantleItemMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestDismantleItems(RequestHandlerData requestHandler, RequestDismantleItemsMessage request, RequestProceedResultDelegate<ResponseDismantleItemsMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseDismantleItemsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanDismentleItem())
            {
                result.InvokeError(new ResponseDismantleItemsMessage());
                return;
            }

            if (!playerCharacter.DismantleItems(request.selectedIndexes, out UITextKeys gameMessage))
            {
                result.InvokeError(new ResponseDismantleItemsMessage()
                {
                    message = gameMessage,
                });
                return;
            }

            result.InvokeSuccess(new ResponseDismantleItemsMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestEnhanceSocketItem(RequestHandlerData requestHandler, RequestEnhanceSocketItemMessage request, RequestProceedResultDelegate<ResponseEnhanceSocketItemMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseEnhanceSocketItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanEnhanceSocketItem())
            {
                result.InvokeError(new ResponseEnhanceSocketItemMessage());
                return;
            }

            if (!playerCharacter.EnhanceSocketItem(request.inventoryType, request.index, request.enhancerId, request.socketIndex, out UITextKeys gameMessage))
            {
                result.InvokeError(new ResponseEnhanceSocketItemMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            result.InvokeSuccess(new ResponseEnhanceSocketItemMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestRefineItem(RequestHandlerData requestHandler, RequestRefineItemMessage request, RequestProceedResultDelegate<ResponseRefineItemMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseRefineItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanRefineItem())
            {
                result.InvokeError(new ResponseRefineItemMessage());
                return;
            }

            if (!playerCharacter.RefineItem(request.inventoryType, request.index, request.enhancerDataIds, out UITextKeys gameMessage))
            {
                result.InvokeError(new ResponseRefineItemMessage()
                {
                    message = gameMessage,
                });
                return;
            }

            result.InvokeSuccess(new ResponseRefineItemMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestRemoveEnhancerFromItem(RequestHandlerData requestHandler, RequestRemoveEnhancerFromItemMessage request, RequestProceedResultDelegate<ResponseRemoveEnhancerFromItemMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseRemoveEnhancerFromItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanRemoveEnhancerFromItem())
            {
                result.InvokeError(new ResponseRemoveEnhancerFromItemMessage());
                return;
            }

            if (!playerCharacter.RemoveEnhancerFromItem(request.inventoryType, request.index, request.socketIndex, out UITextKeys gameMessage))
            {
                result.InvokeError(new ResponseRemoveEnhancerFromItemMessage()
                {
                    message = gameMessage,
                });
                return;
            }

            result.InvokeSuccess(new ResponseRemoveEnhancerFromItemMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestRepairItem(RequestHandlerData requestHandler, RequestRepairItemMessage request, RequestProceedResultDelegate<ResponseRepairItemMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseRepairItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanRepairItem())
            {
                result.InvokeError(new ResponseRepairItemMessage());
                return;
            }

            if (!playerCharacter.RepairItem(request.inventoryType, request.index, out UITextKeys gameMessage))
            {
                result.InvokeError(new ResponseRepairItemMessage()
                {
                    message = gameMessage,
                });
                return;
            }

            result.InvokeSuccess(new ResponseRepairItemMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestRepairEquipItems(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseRepairEquipItemsMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseRepairEquipItemsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanRepairItem())
            {
                result.InvokeError(new ResponseRepairEquipItemsMessage());
                return;
            }

            if (!playerCharacter.RepairEquipItems(out UITextKeys gameMessage))
            {
                result.InvokeError(new ResponseRepairEquipItemsMessage()
                {
                    message = gameMessage,
                });
                return;
            }

            result.InvokeSuccess(new ResponseRepairEquipItemsMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestSellItem(RequestHandlerData requestHandler, RequestSellItemMessage request, RequestProceedResultDelegate<ResponseSellItemMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseSellItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            if (request.amount <= 0)
            {
                result.InvokeError(new ResponseSellItemMessage());
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null)
            {
                if (!playerCharacterEntity.CanSellItem())
                {
                    result.InvokeError(new ResponseSellItemMessage());
                    return;
                }
                if (!playerCharacterEntity.NpcAction.AccessingNpcShopDialog(out _))
                {
                    result.InvokeError(new ResponseSellItemMessage());
                    return;
                }
            }

            if (!playerCharacter.SellItem(request.index, request.amount, out UITextKeys gameMessage))
            {
                result.InvokeError(new ResponseSellItemMessage()
                {
                    message = gameMessage,
                });
                return;
            }

            result.InvokeSuccess(new ResponseSellItemMessage()
            {
                message = gameMessage,
            });
        }

        public async UniTaskVoid HandleRequestSellItems(RequestHandlerData requestHandler, RequestSellItemsMessage request, RequestProceedResultDelegate<ResponseSellItemsMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseSellItemsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null)
            {
                if (!playerCharacterEntity.CanSellItem())
                {
                    result.InvokeError(new ResponseSellItemsMessage());
                    return;
                }
                if (!playerCharacterEntity.NpcAction.AccessingNpcShopDialog(out _))
                {
                    result.InvokeError(new ResponseSellItemsMessage());
                    return;
                }
            }

            if (!playerCharacter.SellItems(request.selectedIndexes, out UITextKeys gameMessage))
            {
                result.InvokeError(new ResponseSellItemsMessage()
                {
                    message = gameMessage,
                });
                return;
            }

            result.InvokeSuccess(new ResponseSellItemsMessage()
            {
                message = gameMessage,
            });
        }
    }
}
