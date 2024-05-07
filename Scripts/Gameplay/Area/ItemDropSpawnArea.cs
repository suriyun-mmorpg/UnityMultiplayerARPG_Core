using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ItemDropSpawnArea : GameSpawnArea<ItemDropEntity>
    {
        public override void RegisterPrefabs()
        {
            base.RegisterPrefabs();
            GameInstance.AddItemDropEntities(prefab);
        }

        protected override ItemDropEntity SpawnInternal(ItemDropEntity prefab, AddressablePrefab addressablePrefab, int level)
        {
            if (!GetRandomPosition(out Vector3 spawnPosition))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
