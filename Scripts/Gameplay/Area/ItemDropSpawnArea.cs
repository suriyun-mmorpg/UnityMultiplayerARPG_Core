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

        protected override ItemDropEntity SpawnInternal(ItemDropEntity prefab, int level)
        {
            if (GetRandomPosition(out Vector3 spawnPosition))
            {
                Quaternion spawnRotation = GetRandomRotation();
                LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    prefab.Identity.HashAssetId,
                    spawnPosition, spawnRotation);
                ItemDropEntity entity = spawnObj.GetComponent<ItemDropEntity>();
                entity.SetSpawnArea(this, prefab, level, spawnPosition);
                entity.InitDropItems();
                BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
                return entity;
            }
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
