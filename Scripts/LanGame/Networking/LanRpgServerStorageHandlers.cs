using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class LanRpgServerStorageHandlers : MonoBehaviour, IServerStorageHandlers
    {
        private readonly ConcurrentDictionary<StorageId, List<CharacterItem>> storageItems = new ConcurrentDictionary<StorageId, List<CharacterItem>>();
        private readonly ConcurrentDictionary<StorageId, HashSet<long>> usingStorageClients = new ConcurrentDictionary<StorageId, HashSet<long>>();
        private readonly ConcurrentDictionary<long, StorageId> usingStorageIds = new ConcurrentDictionary<long, StorageId>();

        public async UniTaskVoid OpenStorage(long connectionId, IPlayerCharacterData playerCharacter, StorageId storageId)
        {
            if (!CanAccessStorage(playerCharacter, storageId))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(connectionId, UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE);
                return;
            }
            // Store storage usage states
            if (!usingStorageClients.ContainsKey(storageId))
                usingStorageClients.TryAdd(storageId, new HashSet<long>());
            usingStorageClients[storageId].Add(connectionId);
            usingStorageIds.TryRemove(connectionId, out _);
            usingStorageIds.TryAdd(connectionId, storageId);
            // Notify storage items to client
            uint storageObjectId;
            Storage storage = GetStorage(storageId, out storageObjectId);
            GameInstance.ServerGameMessageHandlers.NotifyStorageOpened(connectionId, storageId.storageType, storageId.storageOwnerId, storageObjectId, storage.weightLimit, storage.slotLimit);
            List<CharacterItem> storageItems = GetStorageItems(storageId);
            storageItems.FillEmptySlots(storage.slotLimit > 0, storage.slotLimit);
            GameInstance.ServerGameMessageHandlers.NotifyStorageItems(connectionId, storageItems);
            await UniTask.Yield();
        }

        public async UniTaskVoid CloseStorage(long connectionId)
        {
            StorageId storageId;
            if (usingStorageIds.TryGetValue(connectionId, out storageId) && usingStorageClients.ContainsKey(storageId))
            {
                usingStorageClients[storageId].Remove(connectionId);
                usingStorageIds.TryRemove(connectionId, out _);
                GameInstance.ServerGameMessageHandlers.NotifyStorageClosed(connectionId);
            }
            await UniTask.Yield();
        }

        public bool TryGetOpenedStorageId(long connectionId, out StorageId storageId)
        {
            return usingStorageIds.TryGetValue(connectionId, out storageId);
        }

        public async UniTask<CharacterItem> ConvertStorageItems(StorageId storageId, int dataId, short amount, int convertedDataId, short convertedAmount)
        {
            // Prepare storage data
            Storage storage = GetStorage(storageId, out _);
            bool isLimitWeight = storage.weightLimit > 0;
            bool isLimitSlot = storage.slotLimit > 0;
            short weightLimit = storage.weightLimit;
            short slotLimit = storage.slotLimit;
            // Prepare storage items
            List<CharacterItem> storageItems = new List<CharacterItem>(GetStorageItems(storageId));
            // Decrease item from storage
            if (!storageItems.DecreaseItems(dataId, amount, isLimitSlot, out _))
                return null;
            // Increase item to storage
            CharacterItem droppingItem = null;
            if (GameInstance.Items.ContainsKey(convertedDataId) && convertedAmount > 0)
            {
                // Increase item to storage
                droppingItem = CharacterItem.Create(convertedDataId, convertedAmount);
                if (!storageItems.IncreasingItemsWillOverwhelming(convertedDataId, convertedAmount, isLimitWeight, weightLimit, storageItems.GetTotalItemWeight(), isLimitSlot, slotLimit))
                {
                    storageItems.IncreaseItems(droppingItem);
                    droppingItem = null;
                }
            }
            // Update slots
            storageItems.FillEmptySlots(isLimitSlot, slotLimit);
            SetStorageItems(storageId, storageItems);
            NotifyStorageItemsUpdated(storageId.storageType, storageId.storageOwnerId);
            await UniTask.Yield();
            return droppingItem;
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
                    if (GameInstance.ServerBuildingHandlers.TryGetBuilding(storageId.storageOwnerId, out buildingEntity))
                    {
                        objectId = buildingEntity.ObjectId;
                        storage = buildingEntity.Storage;
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
                    if (!GameInstance.ServerBuildingHandlers.TryGetBuilding(storageId.storageOwnerId, out buildingEntity) ||
                        !(buildingEntity.IsCreator(playerCharacter.Id) || buildingEntity.CanUseByEveryone))
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
            return usingStorageClients.ContainsKey(id) && usingStorageClients[id].Count > 0;
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
            usingStorageClients.Clear();
            usingStorageIds.Clear();
        }

        public void NotifyStorageItemsUpdated(StorageType storageType, string storageOwnerId)
        {
            StorageId storageId = new StorageId(storageType, storageOwnerId);
            if (!usingStorageClients.ContainsKey(storageId))
                return;
            GameInstance.ServerGameMessageHandlers.NotifyStorageItemsToClients(usingStorageClients[storageId], GetStorageItems(storageId));
        }

        public IDictionary<StorageId, List<CharacterItem>> GetAllStorageItems()
        {
            return storageItems;
        }
    }
}
