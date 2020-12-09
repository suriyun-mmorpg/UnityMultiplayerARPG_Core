using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ItemDropSpawnArea : GameSpawnArea<ItemDropEntity>
    {
        [System.Serializable]
        public class ItemDropSpawnPrefabData : SpawnPrefabData<ItemDropEntity> { }

        public List<ItemDropSpawnPrefabData> spawningPrefabs = new List<ItemDropSpawnPrefabData>();
        public override SpawnPrefabData<ItemDropEntity>[] SpawningPrefabs
        {
            get { return spawningPrefabs.ToArray(); }
        }

        public override void RegisterPrefabs()
        {
            base.RegisterPrefabs();
            GameInstance.AddItemDropEntities(prefab);
        }

        protected override ItemDropEntity SpawnInternal(ItemDropEntity prefab, short level)
        {
            Vector3 spawnPosition = GetRandomPosition();
            Quaternion spawnRotation = GetRandomRotation();
            GameObject spawnObj = Instantiate(prefab.gameObject, spawnPosition, spawnRotation);
            ItemDropEntity entity = spawnObj.GetComponent<ItemDropEntity>();
            entity.gameObject.SetActive(false);
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            entity.SetSpawnArea(this, prefab, level, spawnPosition);
            return entity;
        }

        public override int GroundLayerMask
        {
            get { return CurrentGameInstance.GetItemDropGroundDetectionLayerMask(); }
        }
    }
}
