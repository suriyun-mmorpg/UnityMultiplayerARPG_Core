using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class LanRpgServerStorageHandlers : MonoBehaviour, IServerStorageHandlers
    {
        private readonly ConcurrentDictionary<StorageId, List<CharacterItem>> storageItems = new ConcurrentDictionary<StorageId, List<CharacterItem>>();
        private readonly ConcurrentDictionary<StorageId, HashSet<long>> usingStorageCharacters = new ConcurrentDictionary<StorageId, HashSet<long>>();

        public async UniTaskVoid OpenStorage(BasePlayerCharacterEntity playerCharacter)
        {
            if (!CanAccessStorage(playerCharacter, playerCharacter.CurrentStorageId))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(playerCharacter.ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }
            if (!usingStorageCharacters.ContainsKey(playerCharacter.CurrentStorageId))
                usingStorageCharacters.TryAdd(playerCharacter.CurrentStorageId, new HashSet<long>());
            usingStorageCharacters[playerCharacter.CurrentStorageId].Add(playerCharacter.ConnectionId);
            // Notify storage items to client
            uint storageObjectId;
            Storage storage = GetStorage(playerCharacter.CurrentStorageId, out storageObjectId);
            BaseGameNetworkManager.Singleton.SendNotifyStorageOpenedToClient(playerCharacter.ConnectionId, playerCharacter.CurrentStorageId.storageType, playerCharacter.CurrentStorageId.storageOwnerId, storageObjectId, storage.weightLimit, storage.slotLimit);
            BaseGameNetworkManager.Singleton.SendNotifyStorageItemsUpdatedToClient(playerCharacter.ConnectionId, GetStorageItems(playerCharacter.CurrentStorageId));
            await UniTask.Yield();
        }

        public async UniTaskVoid CloseStorage(BasePlayerCharacterEntity playerCharacter)
        {
            if (usingStorageCharacters.ContainsKey(playerCharacter.CurrentStorageId))
                usingStorageCharacters[playerCharacter.CurrentStorageId].Remove(playerCharacter.ConnectionId);
            await UniTask.Yield();
        }

        public async UniTask<bool> IncreaseStorageItems(StorageId storageId, CharacterItem addingItem)
        {
            await UniTask.Yield();
            if (addingItem.IsEmptySlot())
                return false;
            List<CharacterItem> storageItems = GetStorageItems(storageId);
            // Prepare storage data
            Storage storage = GetStorage(storageId, out _);
            bool isLimitWeight = storage.weightLimit > 0;
            bool isLimitSlot = storage.slotLimit > 0;
            short weightLimit = storage.weightLimit;
            short slotLimit = storage.slotLimit;
            // Increase item to storage
            bool isOverwhelming = storageItems.IncreasingItemsWillOverwhelming(
                addingItem.dataId, addingItem.amount, isLimitWeight, weightLimit,
                storageItems.GetTotalItemWeight(), isLimitSlot, slotLimit);
            if (!isOverwhelming && storageItems.IncreaseItems(addingItem))
            {
                // Update slots
                storageItems.FillEmptySlots(isLimitSlot, slotLimit);
                SetStorageItems(storageId, storageItems);
                NotifyStorageItemsUpdated(storageId.storageType, storageId.storageOwnerId);
                return true;
            }
            return false;
        }

        public async UniTask<DecreaseStorageItemsResult> DecreaseStorageItems(StorageId storageId, int dataId, short amount)
        {
            await UniTask.Yield();
            List<CharacterItem> storageItems = GetStorageItems(storageId);
            // Prepare storage data
            Storage storage = GetStorage(storageId, out _);
            bool isLimitSlot = storage.slotLimit > 0;
            short slotLimit = storage.slotLimit;
            // Increase item to storage
            Dictionary<int, short> decreaseItems;
            if (storageItems.DecreaseItems(dataId, amount, isLimitSlot, out decreaseItems))
            {
                // Update slots
                storageItems.FillEmptySlots(isLimitSlot, slotLimit);
                SetStorageItems(storageId, storageItems);
                NotifyStorageItemsUpdated(storageId.storageType, storageId.storageOwnerId);
                return new DecreaseStorageItemsResult()
                {
                    IsSuccess = true,
                    DecreasedItems = decreaseItems,
                };
            }
            return new DecreaseStorageItemsResult();
        }

        public List<CharacterItem> GetStorageItems(StorageId storageId)
        {
            if (!storageItems.ContainsKey(storageId))
                storageItems.TryAdd(storageId, new List<CharacterItem>());
            return storageItems[storageId];
        }

        public void SetStorageItems(StorageId storageId, List<CharacterItem> items)
        {
            if (!storageItems.ContainsKey(storageId))
                storageItems.TryAdd(storageId, new List<CharacterItem>());
            storageItems[storageId] = items;
        }

        public Storage GetStorage(StorageId storageId, out uint objectId)
        {
            objectId = 0;
            Storage storage = default(Storage);
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    storage = GameInstance.Singleton.playerStorage;
                    break;
                case StorageType.Guild:
                    storage = GameInstance.Singleton.guildStorage;
                    break;
                case StorageType.Building:
                    StorageEntity buildingEntity;
                    if (BaseGameNetworkManager.Singleton.TryGetBuildingEntity(storageId.storageOwnerId, out buildingEntity))
                    {
                        objectId = buildingEntity.ObjectId;
                        storage = buildingEntity.storage;
                    }
                    break;
            }
            return storage;
        }

        public bool CanAccessStorage(IPlayerCharacterData playerCharacter, StorageId storageId)
        {
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    if (!playerCharacter.UserId.Equals(storageId.storageOwnerId))
                        return false;
                    break;
                case StorageType.Guild:
                    if (!GameInstance.ServerGuildHandlers.ContainsGuild(playerCharacter.GuildId) ||
                        !playerCharacter.GuildId.ToString().Equals(storageId.storageOwnerId))
                        return false;
                    break;
                case StorageType.Building:
                    StorageEntity buildingEntity;
                    if (!BaseGameNetworkManager.Singleton.TryGetBuildingEntity(storageId.storageOwnerId, out buildingEntity) ||
                        !(buildingEntity.IsCreator(playerCharacter.Id) || buildingEntity.canUseByEveryone))
                        return false;
                    break;
            }
            return true;
        }

        public bool IsStorageEntityOpen(StorageEntity storageEntity)
        {
            if (storageEntity == null)
                return false;
            StorageId id = new StorageId(StorageType.Building, storageEntity.Id);
            return usingStorageCharacters.ContainsKey(id) && usingStorageCharacters[id].Count > 0;
        }

        public List<CharacterItem> GetStorageEntityItems(StorageEntity storageEntity)
        {
            if (storageEntity == null)
                return new List<CharacterItem>();
            return GetStorageItems(new StorageId(StorageType.Building, storageEntity.Id));
        }

        public void ClearStorage()
        {
            storageItems.Clear();
            usingStorageCharacters.Clear();
        }

        public void NotifyStorageItemsUpdated(StorageType storageType, string storageOwnerId)
        {
            StorageId storageId = new StorageId(storageType, storageOwnerId);
            BaseGameNetworkManager.Singleton.SendNotifyStorageItemsUpdatedToClients(usingStorageCharacters[storageId], GetStorageItems(storageId));
        }

        public IDictionary<StorageId, List<CharacterItem>> GetAllStorageItems()
        {
            return storageItems;
        }
    }
}
