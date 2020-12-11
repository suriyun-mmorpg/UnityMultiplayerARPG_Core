using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class LanRpgNetworkManager
    {
        private readonly Dictionary<StorageId, List<CharacterItem>> storageItems = new Dictionary<StorageId, List<CharacterItem>>();
        private readonly Dictionary<StorageId, HashSet<uint>> usingStorageCharacters = new Dictionary<StorageId, HashSet<uint>>();

        public void OpenStorage(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (!CanAccessStorage(playerCharacterEntity.CurrentStorageId, playerCharacterEntity))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }
            if (!storageItems.ContainsKey(playerCharacterEntity.CurrentStorageId))
                storageItems[playerCharacterEntity.CurrentStorageId] = new List<CharacterItem>();
            if (!usingStorageCharacters.ContainsKey(playerCharacterEntity.CurrentStorageId))
                usingStorageCharacters[playerCharacterEntity.CurrentStorageId] = new HashSet<uint>();
            usingStorageCharacters[playerCharacterEntity.CurrentStorageId].Add(playerCharacterEntity.ObjectId);
            // Prepare storage data
            Storage storage = GetStorage(playerCharacterEntity.CurrentStorageId);
            bool isLimitSlot = storage.slotLimit > 0;
            short slotLimit = storage.slotLimit;
            storageItems[playerCharacterEntity.CurrentStorageId].FillEmptySlots(isLimitSlot, slotLimit);
            // Update storage items
            playerCharacterEntity.StorageItems = storageItems[playerCharacterEntity.CurrentStorageId].ToArray();
        }

        public void CloseStorage(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (usingStorageCharacters.ContainsKey(playerCharacterEntity.CurrentStorageId))
                usingStorageCharacters[playerCharacterEntity.CurrentStorageId].Remove(playerCharacterEntity.ObjectId);
            playerCharacterEntity.StorageItems = new CharacterItem[0];
        }

        public async UniTask<bool> IncreaseStorageItems(StorageId storageId, CharacterItem addingItem)
        {
            await UniTask.Yield();
            if (addingItem.IsEmptySlot())
                return false;
            List<CharacterItem> storageItems = GetStorageItems(storageId);
            // Prepare storage data
            Storage storage = GetStorage(storageId);
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
                UpdateStorageItemsToCharacters(usingStorageCharacters[storageId], storageItems);
                return true;
            }
            return false;
        }

        public async UniTask<DecreaseStorageItemsResult> DecreaseStorageItems(StorageId storageId, int dataId, short amount)
        {
            await UniTask.Yield();
            List<CharacterItem> storageItemList = GetStorageItems(storageId);
            // Prepare storage data
            Storage storage = GetStorage(storageId);
            bool isLimitSlot = storage.slotLimit > 0;
            short slotLimit = storage.slotLimit;
            // Increase item to storage
            Dictionary<int, short> decreaseItems;
            if (storageItemList.DecreaseItems(dataId, amount, isLimitSlot, out decreaseItems))
            {
                // Update slots
                storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
                UpdateStorageItemsToCharacters(usingStorageCharacters[storageId], storageItemList);
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
                storageItems[storageId] = new List<CharacterItem>();
            return storageItems[storageId];
        }

        public Storage GetStorage(StorageId storageId)
        {
            Storage storage = default(Storage);
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    storage = CurrentGameInstance.playerStorage;
                    break;
                case StorageType.Guild:
                    storage = CurrentGameInstance.guildStorage;
                    break;
                case StorageType.Building:
                    StorageEntity buildingEntity;
                    if (TryGetBuildingEntity(storageId.storageOwnerId, out buildingEntity))
                        storage = buildingEntity.storage;
                    break;
            }
            return storage;
        }

        public bool CanAccessStorage(StorageId storageId, IPlayerCharacterData playerCharacter)
        {
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    if (!playerCharacter.UserId.Equals(storageId.storageOwnerId))
                        return false;
                    break;
                case StorageType.Guild:
                    if (!Guilds.ContainsKey(playerCharacter.GuildId) ||
                        !playerCharacter.GuildId.ToString().Equals(storageId.storageOwnerId))
                        return false;
                    break;
                case StorageType.Building:
                    StorageEntity buildingEntity;
                    if (!TryGetBuildingEntity(storageId.storageOwnerId, out buildingEntity) ||
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
            return usingStorageCharacters.ContainsKey(id) &&
                usingStorageCharacters[id].Count > 0;
        }

        public List<CharacterItem> GetStorageEntityItems(StorageEntity storageEntity)
        {
            if (storageEntity == null)
                return new List<CharacterItem>();
            StorageId id = new StorageId(StorageType.Building, storageEntity.Id);
            if (!storageItems.ContainsKey(id))
                storageItems[id] = new List<CharacterItem>();
            return storageItems[id];
        }

        private void UpdateStorageItemsToCharacters(HashSet<uint> objectIds, List<CharacterItem> storageItems)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (uint objectId in objectIds)
            {
                if (Assets.TryGetSpawnedObject(objectId, out playerCharacterEntity))
                {
                    // Update storage items
                    playerCharacterEntity.StorageItems = storageItems.ToArray();
                }
            }
        }
    }
}
