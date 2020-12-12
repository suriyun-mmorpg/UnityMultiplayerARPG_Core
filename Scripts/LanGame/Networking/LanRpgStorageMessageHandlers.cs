using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class LanRpgStorageMessageHandlers : MonoBehaviour, IServerStorageMessageHandlers
    {
        public IServerPlayerCharacterHandlers ServerPlayerCharacterHandlers { get; set; }
        public IServerStorageHandlers ServerStorageHandlers { get; set; }

        public async UniTaskVoid HandleRequestGetStorageItems(RequestHandlerData requestHandler, RequestGetStorageItemsMessage request, RequestProceedResultDelegate<ResponseGetStorageItemsMessage> result)
        {
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            IPlayerCharacterData playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseGetStorageItemsMessage()
                {
                    error = ResponseGetStorageItemsMessage.Error.CharacterNotFound,
                });
                return;
            }
            if (!ServerStorageHandlers.CanAccessStorage(storageId, playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseGetStorageItemsMessage()
                {
                    error = ResponseGetStorageItemsMessage.Error.NotAllowed,
                });
                return;
            }
            List<CharacterItem> storageItemList = ServerStorageHandlers.GetStorageItems(storageId);
            result.Invoke(AckResponseCode.Success, new ResponseGetStorageItemsMessage()
            {
                storageItems = storageItemList,
            });
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestMoveItemFromStorage(RequestHandlerData requestHandler, RequestMoveItemFromStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemFromStorageMessage> result)
        {
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            short storageItemIndex = request.storageItemIndex;
            short amount = request.amount;
            short inventoryIndex = request.inventoryIndex;
            IPlayerCharacterData playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    error = ResponseMoveItemFromStorageMessage.Error.CharacterNotFound,
                });
                return;
            }
            if (!ServerStorageHandlers.CanAccessStorage(storageId, playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    error = ResponseMoveItemFromStorageMessage.Error.NotAllowed,
                });
                return;
            }
            List<CharacterItem> storageItemList = ServerStorageHandlers.GetStorageItems(storageId);
            if (storageItemIndex < 0 || storageItemIndex >= storageItemList.Count)
            {
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    error = ResponseMoveItemFromStorageMessage.Error.InvalidItemIndex,
                });
                return;
            }
            // Prepare storage data
            Storage storage = ServerStorageHandlers.GetStorage(storageId);
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
            // Success
            result.Invoke(AckResponseCode.Success, new ResponseMoveItemFromStorageMessage());
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestMoveItemToStorage(RequestHandlerData requestHandler, RequestMoveItemToStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemToStorageMessage> result)
        {
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            short inventoryIndex = request.inventoryIndex;
            short amount = request.amount;
            short storageItemIndex = request.storageItemIndex;
            IPlayerCharacterData playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    error = ResponseMoveItemToStorageMessage.Error.CharacterNotFound,
                });
                return;
            }
            if (!ServerStorageHandlers.CanAccessStorage(storageId, playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    error = ResponseMoveItemToStorageMessage.Error.NotAllowed,
                });
                return;
            }
            List<CharacterItem> storageItemList = ServerStorageHandlers.GetStorageItems(storageId);
            if (inventoryIndex < 0 || inventoryIndex >= playerCharacter.NonEquipItems.Count)
            {
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    error = ResponseMoveItemToStorageMessage.Error.InvalidItemIndex,
                });
                return;
            }
            // Prepare storage data
            Storage storage = ServerStorageHandlers.GetStorage(storageId);
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
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeStorageItemMessage()
                {
                    error = ResponseSwapOrMergeStorageItemMessage.Error.CharacterNotFound,
                });
                return;
            }
            if (!ServerStorageHandlers.CanAccessStorage(storageId, playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeStorageItemMessage()
                {
                    error = ResponseSwapOrMergeStorageItemMessage.Error.NotAllowed,
                });
                return;
            }
            List<CharacterItem> storageItemList = ServerStorageHandlers.GetStorageItems(storageId);
            if (fromIndex >= storageItemList.Count ||
                toIndex >= storageItemList.Count)
            {
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeStorageItemMessage()
                {
                    error = ResponseSwapOrMergeStorageItemMessage.Error.InvalidItemIndex,
                });
                return;
            }
            // Prepare storage data
            Storage storage = ServerStorageHandlers.GetStorage(storageId);
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
            // Success
            result.Invoke(AckResponseCode.Success, new ResponseSwapOrMergeStorageItemMessage());
            await UniTask.Yield();
        }
    }
}
