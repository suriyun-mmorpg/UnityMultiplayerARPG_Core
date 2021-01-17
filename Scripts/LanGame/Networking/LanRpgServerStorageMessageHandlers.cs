using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class LanRpgServerStorageMessageHandlers : MonoBehaviour, IServerStorageMessageHandlers
    {
        public async UniTaskVoid HandleRequestOpenStorage(RequestHandlerData requestHandler, RequestOpenStorageMessage request, RequestProceedResultDelegate<ResponseOpenStorageMessage> result)
        {
            if (request.storageType != StorageType.Player &&
                request.storageType != StorageType.Guild)
            {
                result.Invoke(AckResponseCode.Error, new ResponseOpenStorageMessage()
                {
                    error = ResponseOpenStorageMessage.Error.NotAllowed,
                });
                return;
            }
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseOpenStorageMessage()
                {
                    error = ResponseOpenStorageMessage.Error.NotLoggedIn,
                });
                return;
            }
            StorageId storageId;
            if (!playerCharacter.GetStorageId(request.storageType, 0, out storageId))
            {
                result.Invoke(AckResponseCode.Error, new ResponseOpenStorageMessage()
                {
                    error = ResponseOpenStorageMessage.Error.NotAvailable,
                });
                return;
            }
            GameInstance.ServerStorageHandlers.OpenStorage(requestHandler.ConnectionId, playerCharacter, storageId);
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestCloseStorage(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseCloseStorageMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseCloseStorageMessage()
                {
                    error = ResponseCloseStorageMessage.Error.NotLoggedIn,
                });
                return;
            }
            GameInstance.ServerStorageHandlers.CloseStorage(requestHandler.ConnectionId);
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestMoveItemFromStorage(RequestHandlerData requestHandler, RequestMoveItemFromStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemFromStorageMessage> result)
        {
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            short storageItemIndex = request.storageItemIndex;
            short amount = request.storageItemAmount;
            short inventoryIndex = request.inventoryItemIndex;
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    error = ResponseMoveItemFromStorageMessage.Error.NotLoggedIn,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.CannotAccessStorage);
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    error = ResponseMoveItemFromStorageMessage.Error.NotAllowed,
                });
                return;
            }
            List<CharacterItem> storageItemList = GameInstance.ServerStorageHandlers.GetStorageItems(storageId);
            if (storageItemIndex < 0 || storageItemIndex >= storageItemList.Count)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.InvalidItemData);
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    error = ResponseMoveItemFromStorageMessage.Error.InvalidItemIndex,
                });
                return;
            }
            // Prepare storage data
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            bool isLimitSlot = storage.slotLimit > 0;
            short slotLimit = storage.slotLimit;
            // Prepare item data
            CharacterItem movingItem = storageItemList[storageItemIndex].Clone(true);
            IArmorItem equippingArmorItem = movingItem.GetArmorItem();
            IWeaponItem equippingWeaponItem = movingItem.GetWeaponItem();
            IShieldItem equippingShieldItem = movingItem.GetShieldItem();
            movingItem.amount = amount;
            if (inventoryIndex < 0 ||
                inventoryIndex >= playerCharacter.NonEquipItems.Count ||
                playerCharacter.NonEquipItems[inventoryIndex].dataId == movingItem.dataId)
            {
                // Add to inventory or merge
                bool isOverwhelming = playerCharacter.IncreasingItemsWillOverwhelming(movingItem.dataId, movingItem.amount);
                if (isOverwhelming)
                {
                    // Error: cannot carry all items
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.CannotCarryAnymore);
                    result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                    {
                        error = ResponseMoveItemFromStorageMessage.Error.CannotCarryAllItems,
                    });
                    return;
                }
                // Increase to inventory
                playerCharacter.IncreaseItems(movingItem);
                // Decrease from storage
                storageItemList.DecreaseItemsByIndex(storageItemIndex, amount, isLimitSlot);
            }
            else
            {
                // Swapping
                CharacterItem storageItem = storageItemList[storageItemIndex];
                CharacterItem nonEquipItem = playerCharacter.NonEquipItems[inventoryIndex];

                storageItemList[storageItemIndex] = nonEquipItem;
                playerCharacter.NonEquipItems[inventoryIndex] = storageItem;
            }
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            playerCharacter.FillEmptySlots();
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, storageItemList);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            // Success
            result.Invoke(AckResponseCode.Success, new ResponseMoveItemFromStorageMessage());
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestMoveItemToStorage(RequestHandlerData requestHandler, RequestMoveItemToStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemToStorageMessage> result)
        {
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            short inventoryIndex = request.inventoryItemIndex;
            short amount = request.inventoryItemAmount;
            short storageItemIndex = request.storageItemIndex;
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    error = ResponseMoveItemToStorageMessage.Error.NotLoggedIn,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.CannotAccessStorage);
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    error = ResponseMoveItemToStorageMessage.Error.NotAllowed,
                });
                return;
            }
            List<CharacterItem> storageItemList = GameInstance.ServerStorageHandlers.GetStorageItems(storageId);
            if (inventoryIndex < 0 || inventoryIndex >= playerCharacter.NonEquipItems.Count)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.InvalidItemData);
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    error = ResponseMoveItemToStorageMessage.Error.InvalidItemIndex,
                });
                return;
            }
            // Prepare storage data
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            bool isLimitWeight = storage.weightLimit > 0;
            bool isLimitSlot = storage.slotLimit > 0;
            short weightLimit = storage.weightLimit;
            short slotLimit = storage.slotLimit;
            // Prepare item data
            CharacterItem movingItem = playerCharacter.NonEquipItems[inventoryIndex].Clone(true);
            movingItem.amount = amount;
            if (storageItemIndex < 0 ||
                storageItemIndex >= storageItemList.Count ||
                storageItemList[storageItemIndex].dataId == movingItem.dataId)
            {
                // Add to storage or merge
                bool isOverwhelming = storageItemList.IncreasingItemsWillOverwhelming(
                    movingItem.dataId, movingItem.amount, isLimitWeight, weightLimit,
                    storageItemList.GetTotalItemWeight(), isLimitSlot, slotLimit);
                if (isOverwhelming)
                {
                    // Error: cannot store all items
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.CannotCarryAnymore);
                    result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                    {
                        error = ResponseMoveItemToStorageMessage.Error.CannotCarryAllItems,
                    });
                    return;
                }
                // Increase to storage
                storageItemList.IncreaseItems(movingItem);
                // Decrease from inventory
                playerCharacter.DecreaseItemsByIndex(inventoryIndex, amount);
            }
            else
            {
                // Swapping
                CharacterItem storageItem = storageItemList[storageItemIndex];
                CharacterItem nonEquipItem = playerCharacter.NonEquipItems[inventoryIndex];

                storageItemList[storageItemIndex] = nonEquipItem;
                playerCharacter.NonEquipItems[inventoryIndex] = storageItem;
            }
            playerCharacter.FillEmptySlots();
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, storageItemList);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            // Success
            result.Invoke(AckResponseCode.Success, new ResponseMoveItemToStorageMessage());
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestSwapOrMergeStorageItem(RequestHandlerData requestHandler, RequestSwapOrMergeStorageItemMessage request, RequestProceedResultDelegate<ResponseSwapOrMergeStorageItemMessage> result)
        {
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            short fromIndex = request.fromIndex;
            short toIndex = request.toIndex;
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeStorageItemMessage()
                {
                    error = ResponseSwapOrMergeStorageItemMessage.Error.NotLoggedIn,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.CannotAccessStorage);
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeStorageItemMessage()
                {
                    error = ResponseSwapOrMergeStorageItemMessage.Error.NotAllowed,
                });
                return;
            }
            List<CharacterItem> storageItemList = GameInstance.ServerStorageHandlers.GetStorageItems(storageId);
            if (fromIndex >= storageItemList.Count ||
                toIndex >= storageItemList.Count)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.InvalidItemData);
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeStorageItemMessage()
                {
                    error = ResponseSwapOrMergeStorageItemMessage.Error.InvalidItemIndex,
                });
                return;
            }
            // Prepare storage data
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            bool isLimitSlot = storage.slotLimit > 0;
            short slotLimit = storage.slotLimit;
            // Prepare item data
            CharacterItem fromItem = storageItemList[fromIndex];
            CharacterItem toItem = storageItemList[toIndex];

            if (fromItem.dataId.Equals(toItem.dataId) && !fromItem.IsFull() && !toItem.IsFull())
            {
                // Merge if same id and not full
                short maxStack = toItem.GetMaxStack();
                if (toItem.amount + fromItem.amount <= maxStack)
                {
                    toItem.amount += fromItem.amount;
                    storageItemList[fromIndex] = CharacterItem.Empty;
                    storageItemList[toIndex] = toItem;
                }
                else
                {
                    short remains = (short)(toItem.amount + fromItem.amount - maxStack);
                    toItem.amount = maxStack;
                    fromItem.amount = remains;
                    storageItemList[fromIndex] = fromItem;
                    storageItemList[toIndex] = toItem;
                }
            }
            else
            {
                // Swap
                storageItemList[fromIndex] = toItem;
                storageItemList[toIndex] = fromItem;
            }
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, storageItemList);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            // Success
            result.Invoke(AckResponseCode.Success, new ResponseSwapOrMergeStorageItemMessage());
            await UniTask.Yield();
        }
    }
}
