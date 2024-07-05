using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class HarvestableSpawnArea : GameSpawnArea<HarvestableEntity>
    {
        [Tooltip("This is deprecated, might be removed in future version, set your asset to `Asset` instead.")]
        [ReadOnlyField]
        public HarvestableEntity harvestableEntity;

        protected override void Awake()
        {
            base.Awake();
            MigrateAsset();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            MigrateAsset();
        }
#endif

        private void MigrateAsset()
        {
            if (prefab == null && harvestableEntity != null)
            {
                prefab = harvestableEntity;
                harvestableEntity = null;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
            if (prefab != null && harvestableEntity != null)
            {
                harvestableEntity = null;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public override void RegisterPrefabs()
        {
            base.RegisterPrefabs();
            GameInstance.AddHarvestableEntities(prefab);
        }

        protected override HarvestableEntity SpawnInternal(HarvestableEntity prefab, AddressablePrefab addressablePrefab, int level)
        {
            if (!GetRandomPosition(out Vector3 spawnPosition))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Logging.LogWarning(ToString(), $"Cannot spawn harvestable, it cannot find grounded position, pending harvestable amount {_pending.Count}");
#endif
                return null;
            }

            Quaternion spawnRotation = GetRandomRotation();
            LiteNetLibIdentity spawnObj = null;
            HarvestableEntity entity = null;
            if (prefab != null)
            {
                spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    prefab.Identity.HashAssetId,
                    spawnPosition, spawnRotation);
                if (spawnObj == null)
                    return null;
                entity = spawnObj.GetComponent<HarvestableEntity>();
                entity.SetSpawnArea(this, prefab, level, spawnPosition);
            }
            else if (addressablePrefab.IsDataValid())
            {
                spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    addressablePrefab.HashAssetId,
                    spawnPosition, spawnRotation);
                if (spawnObj == null)
                    return null;
                entity = spawnObj.GetComponent<HarvestableEntity>();
                entity.SetSpawnArea(this, addressablePrefab, level, spawnPosition);
            }

            if (entity == null)
            {
                Logging.LogWarning(ToString(), $"Cannot spawn harvestable, entity is null");
                return null;
            }

            if (IsOverlapSomethingNearby(spawnPosition, entity))
            {
                // Destroy the entity (because it is hitting something)
                BaseGameNetworkManager.Singleton.Assets.DestroyObjectInstance(spawnObj);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Logging.LogWarning(ToString(), $"Cannot spawn harvestable, it is hitting something nearby, pending monster amount {_pending.Count}");
#endif
                return null;
            }

            entity.InitStats();
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            return entity;
        }

        public bool IsOverlapSomethingNearby(Vector3 position, HarvestableEntity entity)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
            {
                Collider2D[] overlaps = Physics2D.OverlapCircleAll(position, entity.ColliderDetectionRadius);
                foreach (Collider2D overlap in overlaps)
                {
                    if (overlap.gameObject.layer == CurrentGameInstance.playerLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.playingLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.monsterLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.npcLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.vehicleLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.itemDropLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.buildingLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.harvestableLayer)
                    {
                        // Don't spawn because it will hitting other entities
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Logging.LogWarning(ToString(), $"Cannot spawn harvestable, it is collided to another entities, pending harvestable amount {_pending.Count}");
#endif
                        return true;
                    }
                }
            }
            else
            {
                Collider[] overlaps = Physics.OverlapSphere(position, entity.ColliderDetectionRadius);
                foreach (Collider overlap in overlaps)
                {
                    if (overlap.gameObject.layer == CurrentGameInstance.playerLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.playingLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.monsterLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.npcLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.vehicleLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.itemDropLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.buildingLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.harvestableLayer)
                    {
                        // Don't spawn because it will hitting other entities
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Logging.LogWarning(ToString(), $"Cannot spawn harvestable, it is collided to another entities, pending harvestable amount {_pending.Count}");
#endif
                        return true;
                    }
                }
            }
            return false;
        }

        public override int GroundLayerMask
        {
            get { return CurrentGameInstance.GetHarvestableSpawnGroundDetectionLayerMask(); }
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
