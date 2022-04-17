using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.DEFAULT_GAME_SAVE_SYSTEM_FILE, menuName = GameDataMenuConsts.DEFAULT_GAME_SAVE_SYSTEM_MENU, order = GameDataMenuConsts.DEFAULT_GAME_SAVE_SYSTEM_ORDER)]
    public class DefaultGameSaveSystem : BaseGameSaveSystem
    {
        private readonly WorldSaveData worldSaveData = new WorldSaveData();
        private readonly SummonBuffsSaveData summonBuffsSaveData = new SummonBuffsSaveData();
        private readonly StorageSaveData hostStorageSaveData = new StorageSaveData();
        private readonly StorageSaveData playerStorageSaveData = new StorageSaveData();
        private readonly Dictionary<StorageId, List<CharacterItem>> playerStorageItems = new Dictionary<StorageId, List<CharacterItem>>();
        private bool isReadyToSave;

        public override void OnServerStart()
        {
            isReadyToSave = false;
        }

        public override async UniTask PreSpawnEntities(IPlayerCharacterData hostPlayerCharacterData, IDictionary<StorageId, List<CharacterItem>> storageItems)
        {
            isReadyToSave = false;
            storageItems.Clear();
            if (hostPlayerCharacterData != null && !string.IsNullOrEmpty(hostPlayerCharacterData.Id))
            {
                // Load and Spawn buildings
                worldSaveData.LoadPersistentData(hostPlayerCharacterData.Id, BaseGameNetworkManager.CurrentMapInfo.Id);
                foreach (BuildingSaveData building in worldSaveData.buildings)
                {
                    BaseGameNetworkManager.Singleton.CreateBuildingEntity(building, true);
                }
                // Load storage data
                hostStorageSaveData.LoadPersistentData(hostPlayerCharacterData.Id);
                StorageId storageId;
                foreach (StorageCharacterItem storageItem in hostStorageSaveData.storageItems)
                {
                    storageId = new StorageId(storageItem.storageType, storageItem.storageOwnerId);
                    if (!storageItems.ContainsKey(storageId))
                        storageItems[storageId] = new List<CharacterItem>();
                    storageItems[storageId].Add(storageItem.characterItem);
                }
            }
            isReadyToSave = true;
            await UniTask.Yield();
        }

        public override void SaveCharacter(IPlayerCharacterData playerCharacterData)
        {
            playerCharacterData.SavePersistentCharacterData();
        }

        public override List<PlayerCharacterData> LoadCharacters()
        {
            return PlayerCharacterDataExtensions.LoadAllPersistentCharacterData();
        }

        public override List<CharacterBuff> LoadSummonBuffs(IPlayerCharacterData playerCharacterData)
        {
            summonBuffsSaveData.LoadPersistentData(playerCharacterData.Id);
            return summonBuffsSaveData.summonBuffs;
        }

        public override List<CharacterItem> LoadPlayerStorage(IPlayerCharacterData playerCharacterData)
        {
            List<CharacterItem> result = new List<CharacterItem>();
            playerStorageItems.Clear();
            if (playerCharacterData != null && !string.IsNullOrEmpty(playerCharacterData.Id))
            {
                // Load storage data
                playerStorageSaveData.LoadPersistentData(playerCharacterData.Id);
                StorageId storageId;
                foreach (StorageCharacterItem storageItem in playerStorageSaveData.storageItems)
                {
                    storageId = new StorageId(storageItem.storageType, storageItem.storageOwnerId);
                    if (!playerStorageItems.ContainsKey(storageId))
                        playerStorageItems[storageId] = new List<CharacterItem>();
                    playerStorageItems[storageId].Add(storageItem.characterItem);
                }
                storageId = new StorageId(StorageType.Player, playerCharacterData.Id);
                if (playerStorageItems.ContainsKey(storageId))
                {
                    // Result is storage items for the character only
                    result = playerStorageItems[storageId];
                }
            }
            return result;
        }

        public override void SaveStorage(IPlayerCharacterData hostPlayerCharacterData, IDictionary<StorageId, List<CharacterItem>> storageItems)
        {
            if (!isReadyToSave)
                return;

            hostStorageSaveData.storageItems.Clear();
            foreach (StorageId storageId in storageItems.Keys)
            {
                if (storageId.storageType == StorageType.Player &&
                    !storageId.storageOwnerId.Equals(hostPlayerCharacterData.Id))
                {
                    // Non-host player's storage will be saved in `SavePlayerStorage` function
                    continue;
                }
                foreach (CharacterItem storageItem in storageItems[storageId])
                {
                    hostStorageSaveData.storageItems.Add(new StorageCharacterItem()
                    {
                        storageType = storageId.storageType,
                        storageOwnerId = storageId.storageOwnerId,
                        characterItem = storageItem,
                    });
                }
            }
            hostStorageSaveData.SavePersistentData(hostPlayerCharacterData.Id);
        }

        public override void SavePlayerStorage(IPlayerCharacterData playerCharacterData, List<CharacterItem> storageItems)
        {
            for (int i = playerStorageSaveData.storageItems.Count - 1; i >= 0; --i)
            {
                if (playerStorageSaveData.storageItems[i].storageType == StorageType.Player &&
                    playerStorageSaveData.storageItems[i].storageOwnerId.Equals(playerCharacterData.Id))
                    playerStorageSaveData.storageItems.RemoveAt(i);
            }
            foreach (CharacterItem storageItem in storageItems)
            {
                playerStorageSaveData.storageItems.Add(new StorageCharacterItem()
                {
                    storageType = StorageType.Player,
                    storageOwnerId = playerCharacterData.Id,
                    characterItem = storageItem,
                });
            }
            playerStorageSaveData.SavePersistentData(playerCharacterData.Id);
        }

        public override void SaveWorld(IPlayerCharacterData hostPlayerCharacterData, IEnumerable<IBuildingSaveData> buildings)
        {
            if (!isReadyToSave)
                return;

            // Save building entities / Tree / Rocks
            worldSaveData.buildings.Clear();
            foreach (IBuildingSaveData buildingEntity in buildings)
            {
                if (buildingEntity == null) continue;
                worldSaveData.buildings.Add(new BuildingSaveData()
                {
                    Id = buildingEntity.Id,
                    ParentId = buildingEntity.ParentId,
                    EntityId = buildingEntity.EntityId,
                    Position = buildingEntity.Position,
                    Rotation = buildingEntity.Rotation,
                    CurrentHp = buildingEntity.CurrentHp,
                    IsLocked = buildingEntity.IsLocked,
                    LockPassword = buildingEntity.LockPassword,
                    CreatorId = buildingEntity.CreatorId,
                    CreatorName = buildingEntity.CreatorName,
                    ExtraData = buildingEntity.ExtraData,
                });
            }
            worldSaveData.SavePersistentData(hostPlayerCharacterData.Id, BaseGameNetworkManager.CurrentMapInfo.Id);
        }

        public override void SaveSummonBuffs(IPlayerCharacterData playerCharacterData, List<CharacterSummon> summons)
        {
            if (!isReadyToSave)
                return;

            // Save buffs from all summons
            summonBuffsSaveData.summonBuffs.Clear();
            CharacterSummon tempSummon;
            CharacterBuff tempBuff;
            for (int i = 0; i < summons.Count; ++i)
            {
                tempSummon = summons[i];
                if (tempSummon == null || tempSummon.CacheEntity == null || tempSummon.CacheEntity.Buffs == null || tempSummon.CacheEntity.Buffs.Count == 0) continue;
                for (int j = 0; j < tempSummon.CacheEntity.Buffs.Count; ++j)
                {
                    tempBuff = tempSummon.CacheEntity.Buffs[j];
                    summonBuffsSaveData.summonBuffs.Add(new CharacterBuff()
                    {
                        id = i + "_" + j,
                        type = tempBuff.type,
                        dataId = tempBuff.dataId,
                        level = tempBuff.level,
                        buffRemainsDuration = tempBuff.buffRemainsDuration,
                    });
                }
            }
            summonBuffsSaveData.SavePersistentData(playerCharacterData.Id);
        }

        public override void OnSceneChanging()
        {
            isReadyToSave = false;
        }
    }
}
