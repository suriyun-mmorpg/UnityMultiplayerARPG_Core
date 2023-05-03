using LiteNetLibManager;
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

        protected override ItemDropEntity SpawnInternal(ItemDropEntity prefab, int level)
        {
            Vector3 spawnPosition;
            if (GetRandomPosition(out spawnPosition))
            {
                Quaternion spawnRotation = GetRandomRotation();
                LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    prefab.Identity.HashAssetId,
                    spawnPosition, spawnRotation);
                ItemDropEntity entity = spawnObj.GetComponent<ItemDropEntity>();
                entity.SetSpawnArea(this, prefab, level, spawnPosition);
                BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
                return entity;
            }
            _pending.Add(new ItemDropSpawnPrefabData()
            {
                prefab = prefab,
                level = level,
                amount = 1
            });
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Logging.LogWarning(ToString(), $"Cannot spawn item drop, it cannot find grounded position, pending item drop amount {_pending.Count}");
#endif
            return null;
        }

        public override int GroundLayerMask
        {
            get { return CurrentGameInstance.GetItemDropGroundDetectionLayerMask(); }
        }

#if UNITY_EDITOR
        [ContextMenu("Count Spawning Objects")]
        public override void CountSpawningObjects()
        {
            base.CountSpawningObjects();
        }
#endif
    }
}
