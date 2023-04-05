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
        private readonly HashSet<StorageId> busyStorages = new HashSet<StorageId>();

        public async UniTaskVoid OpenStorage(long connectionId, IPlayerCharacterData playerCharacter, StorageId storageId)
        {
            await UniTask.Yield();
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
            Storage storage = GetStorage(storageId, out uint storageObjectId);
            GameInstance.ServerGameMessageHandlers.NotifyStorageOpened(connectionId, storageId.storageType, storageId.storageOwnerId, storageObjectId, storage.weightLimit, storage.slotLimit);
            List<CharacterItem> storageItems = GetStorageItems(storageId);
            storageItems.FillEmptySlots(storage.slotLimit > 0, storage.slotLimit);
            GameInstance.ServerGameMessageHandlers.NotifyStorageItems(connectionId, storageItems);
        }

        public async UniTaskVoid CloseStorage(long connectionId)
        {
            await UniTask.Yield();
            if (usingStorageIds.TryGetValue(connectionId, out StorageId storageId) && usingStorageClients.ContainsKey(storageId))
            {
                usingStorageClients[storageId].Remove(connectionId);
                usingStorageIds.TryRemove(connectionId, out _);
                GameInstance.ServerGameMessageHandlers.NotifyStorageClosed(connectionId);
            }
        }

        public bool TryGetOpenedStorageId(long connectionId, out StorageId storageId)
        {
            return usingStorageIds.TryGetValue(connectionId, out storageId);
        }

        public async UniTask<List<CharacterItem>> ConvertStorageItems(StorageId storageId, List<StorageConvertItemsEntry> convertItems)
        {
            await UniTask.Yield();
            // Prepare storage data
            Storage storage = GetStorage(storageId, out _);
            bool isLimitWeight = storage.weightLimit > 0;
            bool isLimitSlot = storage.slotLimit > 0;
            int weightLimit = storage.weightLimit;
            int slotLimit = storage.slotLimit;
            // Prepare storage items
            List<CharacterItem> storageItems = new List<CharacterItem>(GetStorageItems(storageId));
            List<CharacterItem> droppingItems = new List<CharacterItem>();
            for (int i = 0; i < convertItems.Count; ++i)
            {
                int dataId = convertItems[i].dataId;
                int amount = convertItems[i].amount;
                int convertedDataId = convertItems[i].convertedDataId;
                int convertedAmount = convertItems[i].convertedAmount;
                // Decrease item from storage
                if (!storageItems.DecreaseItems(dataId, amount, isLimitSlot, out _))
                    continue;
                // Increase item to storage
                if (GameInstance.Items.ContainsKey(convertedDataId) && convertedAmount > 0)
                {
                    // Increase item to storage
                    CharacterItem droppingItem = CharacterItem.Create(convertedDataId, 1, convertedAmount);
                    if (!storageItems.IncreasingItemsWillOverwhelming(convertedDataId, convertedAmount, isLimitWeight, weightLimit, storageItems.GetTotalItemWeight(), isLimitSlot, slotLimit))
                    {
                        storageItems.IncreaseItems(droppingItem);
                    }
                    else
                    {
                        droppingItems.Add(droppingItem);
                    }
                }
            }
            // Update slots
            storageItems.FillEmptySlots(isLimitSlot, slotLimit);
            SetStorageItems(storageId, storageItems);
            NotifyStorageItemsUpdated(storageId.storageType, storageId.storageOwnerId);
            return droppingItems;
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
            Storage storage = default;
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    storage = GameInstance.Singleton.playerStorage;
                    break;
                case StorageType.Guild:
                    storage = GameInstance.Singleton.guildStorage;
                    break;
                case StorageType.Building:
                    if (GameInstance.ServerBuildingHandlers.TryGetBuilding(storageId.storageOwnerId, out StorageEntity buildingEntity))
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
                    if (!GameInstance.ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out GuildData guild) ||
                        !playerCharacter.GuildId.ToString().Equals(storageId.storageOwnerId) || !guild.CanUseStorage(playerCharacter.Id))
                        return false;
                    break;
                case StorageType.Building:
                    if (!GameInstance.ServerBuildingHandlers.TryGetBuilding(storageId.storageOwnerId, out StorageEntity buildingEntity) ||
                        !(buildingEntity.IsCreator(playerCharacter.Id) || buildingEntity.CanUseByEveryone))
                        return false;
                    break;
                default:
                    return false;
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
            busyStorages.Clear();
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

        public void SetStorageBusy(StorageId storageId, bool isBusy)
        {
            if (isBusy)
                busyStorages.Add(storageId);
            else
                busyStorages.Remove(storageId);
        }

        public bool IsStorageBusy(StorageId storageId)
        {
            return busyStorages.Contains(storageId);
        }
    }
}
