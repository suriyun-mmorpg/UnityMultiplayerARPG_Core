using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Default Lan Rpg Save System", menuName = "Create Offline Save System/Default Lan Rpg Save System", order = -2499)]
    public class DefaultLanRpgSaveSystem : BaseLanRpgSaveSystem
    {
        private readonly WorldSaveData worldSaveData = new WorldSaveData();
        private readonly StorageSaveData storageSaveData = new StorageSaveData();
        private bool isReadyToSave;

        public override void OnServerStart(BaseGameNetworkManager manager)
        {
            isReadyToSave = false;
        }

        public override void OnServerOnlineSceneLoaded(BaseGameNetworkManager manager, IPlayerCharacterData hostPlayerCharacterData, Dictionary<string, BuildingEntity> buildingEntities, Dictionary<StorageId, List<CharacterItem>> storageItems)
        {
            manager.StartCoroutine(OnServerOnlineSceneLoadedRoutine(manager, hostPlayerCharacterData, buildingEntities, storageItems));
        }

        private IEnumerator OnServerOnlineSceneLoadedRoutine(BaseGameNetworkManager manager, IPlayerCharacterData hostPlayerCharacterData, Dictionary<string, BuildingEntity> buildingEntities, Dictionary<StorageId, List<CharacterItem>> storageItems)
        {
            while (!manager.IsReadyToInstantiateObjects())
            {
                yield return null;
            }
            buildingEntities.Clear();
            storageItems.Clear();
            // Load and Spawn buildings
            worldSaveData.LoadPersistentData(hostPlayerCharacterData.Id, BaseGameNetworkManager.CurrentMapInfo.Id);
            yield return null;
            foreach (BuildingSaveData building in worldSaveData.buildings)
            {
                manager.CreateBuildingEntity(building, true);
            }
            // Load storage data
            storageSaveData.LoadPersistentData(hostPlayerCharacterData.Id);
            yield return null;
            StorageId storageId;
            foreach (StorageCharacterItem storageItem in storageSaveData.storageItems)
            {
                storageId = new StorageId(storageItem.storageType, storageItem.storageOwnerId);
                if (!storageItems.ContainsKey(storageId))
                    storageItems[storageId] = new List<CharacterItem>();
                storageItems[storageId].Add(storageItem.characterItem);
            }
            // Spawn harvestables
            HarvestableSpawnArea[] harvestableSpawnAreas = FindObjectsOfType<HarvestableSpawnArea>();
            foreach (HarvestableSpawnArea harvestableSpawnArea in harvestableSpawnAreas)
            {
                harvestableSpawnArea.SpawnAll();
            }
            isReadyToSave = true;
        }

        public override void SaveCharacter(BaseGameNetworkManager manager, IPlayerCharacterData playerCharacterData)
        {
            playerCharacterData.SavePersistentCharacterData();
        }

        public override void SaveCreatingCharacter(IPlayerCharacterData playerCharacterData)
        {
            playerCharacterData.SavePersistentCharacterData();
        }

        public override List<PlayerCharacterData> LoadCharacters()
        {
            return PlayerCharacterDataExtension.LoadAllPersistentCharacterData();
        }

        public override void SaveStorage(BaseGameNetworkManager manager, IPlayerCharacterData hostPlayerCharacterData, Dictionary<StorageId, List<CharacterItem>> storageItems)
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

        public override void SaveWorld(BaseGameNetworkManager manager, IPlayerCharacterData hostPlayerCharacterData, Dictionary<string, BuildingEntity> buildingEntities)
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
                    DataId = buildingEntity.DataId,
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
