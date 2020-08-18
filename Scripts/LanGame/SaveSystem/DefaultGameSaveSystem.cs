using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Default Game Save System", menuName = "Create Game Save System/Default Game Save System", order = -2499)]
    public class DefaultGameSaveSystem : BaseGameSaveSystem
    {
        private readonly WorldSaveData worldSaveData = new WorldSaveData();
        private readonly StorageSaveData storageSaveData = new StorageSaveData();
        private bool isReadyToSave;

        public override void OnServerStart()
        {
            isReadyToSave = false;
        }

        public override async UniTask PreSpawnEntities(IPlayerCharacterData hostPlayerCharacterData, Dictionary<string, BuildingEntity> buildingEntities, Dictionary<StorageId, List<CharacterItem>> storageItems)
        {
            isReadyToSave = false;
            buildingEntities.Clear();
            storageItems.Clear();
            if (hostPlayerCharacterData != null && !string.IsNullOrEmpty(hostPlayerCharacterData.Id))
            {
                // Load and Spawn buildings
                worldSaveData.LoadPersistentData(hostPlayerCharacterData.Id, BaseGameNetworkManager.CurrentMapInfo.Id);
                await UniTask.Yield();
                foreach (BuildingSaveData building in worldSaveData.buildings)
                {
                    BaseGameNetworkManager.Singleton.CreateBuildingEntity(building, true);
                }
                // Load storage data
                storageSaveData.LoadPersistentData(hostPlayerCharacterData.Id);
                await UniTask.Yield();
                StorageId storageId;
                foreach (StorageCharacterItem storageItem in storageSaveData.storageItems)
                {
                    storageId = new StorageId(storageItem.storageType, storageItem.storageOwnerId);
                    if (!storageItems.ContainsKey(storageId))
                        storageItems[storageId] = new List<CharacterItem>();
                    storageItems[storageId].Add(storageItem.characterItem);
                }
            }
            isReadyToSave = true;
        }

        public override void SaveCharacter(IPlayerCharacterData playerCharacterData)
        {
            playerCharacterData.SavePersistentCharacterData();
        }

        public override List<PlayerCharacterData> LoadCharacters()
        {
            return PlayerCharacterDataExtension.LoadAllPersistentCharacterData();
        }

        public override void SaveStorage(IPlayerCharacterData hostPlayerCharacterData, Dictionary<StorageId, List<CharacterItem>> storageItems)
        {
            if (!isReadyToSave)
                return;
            
            storageSaveData.storageItems.Clear();
            foreach (StorageId key in storageItems.Keys)
            {
                foreach (CharacterItem item in storageItems[key])
                {
                    storageSaveData.storageItems.Add(new StorageCharacterItem()
                    {
                        storageType = key.storageType,
                        storageOwnerId = key.storageOwnerId,
                        characterItem = item,
                    });
                }
            }
            storageSaveData.SavePersistentData(hostPlayerCharacterData.Id);
        }

        public override void SaveWorld(IPlayerCharacterData hostPlayerCharacterData, Dictionary<string, BuildingEntity> buildingEntities)
        {
            if (!isReadyToSave)
                return;

            // Save building entities / Tree / Rocks
            worldSaveData.buildings.Clear();
            foreach (BuildingEntity buildingEntity in buildingEntities.Values)
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
                });
            }
            worldSaveData.SavePersistentData(hostPlayerCharacterData.Id, BaseGameNetworkManager.CurrentMapInfo.Id);
        }
    }
}
