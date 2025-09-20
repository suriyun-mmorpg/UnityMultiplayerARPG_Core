using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ItemDropSpawnArea : GameSpawnArea<ItemDropEntity>
    {
        public override void RegisterPrefabs()
        {
            base.RegisterPrefabs();
#if !EXCLUDE_PREFAB_REFS
            GameInstance.AddItemDropEntities(prefab);
#endif
            GameInstance.AddAssetReferenceItemDropEntities(addressablePrefab);
        }

        protected override ItemDropEntity SpawnInternal(ItemDropEntity prefab, AddressablePrefab addressablePrefab, int level, float destroyRespawnDelay)
        {
            if (!GetRandomPosition(out Vector3 spawnPosition))
            {
#if UNITY_EDITOR || DEBUG_SPAWN_AREA
                Logging.LogWarning(ToString(), $"Cannot spawn item drop, it cannot find grounded position, pending item drop amount {_pending.Count}");
#endif
                return null;
            }

            Quaternion spawnRotation = GetRandomRotation();
            LiteNetLibIdentity spawnObj = null;
            ItemDropEntity entity = null;
            if (prefab != null)
            {
                spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    prefab.Identity.HashAssetId,
                    spawnPosition, spawnRotation);
                if (spawnObj == null)
                    return null;
                entity = spawnObj.GetComponent<ItemDropEntity>();
                entity.SetSpawnArea(this, prefab, level, spawnPosition);
                if (destroyRespawnDelay > 0f)
                    entity.DestroyRespawnDelay = destroyRespawnDelay;
            }
            else if (addressablePrefab.IsDataValid())
            {
                spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    addressablePrefab.HashAssetId,
                    spawnPosition, spawnRotation);
                if (spawnObj == null)
                    return null;
                entity = spawnObj.GetComponent<ItemDropEntity>();
                entity.SetSpawnArea(this, addressablePrefab, level, spawnPosition);
                if (destroyRespawnDelay > 0f)
                    entity.DestroyRespawnDelay = destroyRespawnDelay;
            }

            if (entity == null)
            {
                Logging.LogWarning(ToString(), $"Cannot spawn item drop, entity is null");
                return null;
            }

            entity.Init();
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            return entity;
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
