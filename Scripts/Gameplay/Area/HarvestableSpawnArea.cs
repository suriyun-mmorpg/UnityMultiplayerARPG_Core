using Insthync.AddressableAssetTools;
using Insthync.UnityEditorUtils;
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

        protected Collider2D[] _overlaps2D = new Collider2D[128];
        protected Collider[] _overlaps3D = new Collider[128];

        protected override void Awake()
        {
            base.Awake();
            MigrateAsset();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            MigrateAsset();
            base.OnValidate();
        }
#endif

        private void MigrateAsset()
        {
#if !EXCLUDE_PREFAB_REFS || DISABLE_ADDRESSABLES
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
#endif
        }

        public override void RegisterPrefabs()
        {
            base.RegisterPrefabs();
#if !EXCLUDE_PREFAB_REFS || DISABLE_ADDRESSABLES
            GameInstance.AddHarvestableEntities(prefab);
            foreach (SpawnPrefabData spawnData in spawningPrefabs)
            {
                GameInstance.AddHarvestableEntities(spawnData.prefab);
            }
#endif
#if !DISABLE_ADDRESSABLES
            GameInstance.AddAssetReferenceHarvestableEntities(addressablePrefab);
            foreach (SpawnPrefabData spawnData in spawningPrefabs)
            {
                GameInstance.AddAssetReferenceHarvestableEntities(spawnData.addressablePrefab);
            }
#endif
        }

        protected override HarvestableEntity SpawnInternal(HarvestableEntity prefab
#if !DISABLE_ADDRESSABLES
            , AddressablePrefab addressablePrefab
#endif
            , int level, float destroyRespawnDelay)
        {
            if (!GetRandomPosition(out Vector3 spawnPosition))
            {
#if UNITY_EDITOR || DEBUG_SPAWN_AREA
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
                spawnObj.SubChannelId = subChannelId;
                entity = spawnObj.GetComponent<HarvestableEntity>();
                entity.SetSpawnArea(this, prefab, level, spawnPosition);
                if (destroyRespawnDelay > 0f)
                    entity.DestroyRespawnDelay = destroyRespawnDelay;
            }
#if !DISABLE_ADDRESSABLES
            else if (addressablePrefab.IsDataValid())
            {
                spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    addressablePrefab.HashAssetId,
                    spawnPosition, spawnRotation);
                if (spawnObj == null)
                    return null;
                spawnObj.SubChannelId = subChannelId;
                entity = spawnObj.GetComponent<HarvestableEntity>();
                entity.SetSpawnArea(this, addressablePrefab, level, spawnPosition);
                if (destroyRespawnDelay > 0f)
                    entity.DestroyRespawnDelay = destroyRespawnDelay;
            }
#endif

            if (entity == null)
            {
                Logging.LogWarning(ToString(), $"Cannot spawn harvestable, entity is null");
                return null;
            }

            if (IsOverlapSomethingNearby(spawnPosition, entity))
            {
                // Destroy the entity (because it is hitting something)
                BaseGameNetworkManager.Singleton.Assets.DestroyObjectInstance(spawnObj);
#if UNITY_EDITOR || DEBUG_SPAWN_AREA
                Logging.LogWarning(ToString(), $"Cannot spawn harvestable, it is hitting something nearby, pending monster amount {_pending.Count}");
#endif
                return null;
            }

            entity.InitStats();
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            entity.CallRpcOnSpawned();
            return entity;
        }

        public bool IsOverlapSomethingNearby(Vector3 position, HarvestableEntity entity)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
            {
                ContactFilter2D contactFilter2D = new ContactFilter2D();
                int hitCount = Physics2D.OverlapCircle(position, entity.ColliderDetectionRadius, contactFilter2D, _overlaps2D);
                for (int i = 0; i < hitCount; ++i)
                {
                    Collider2D overlap = _overlaps2D[i];
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
#if UNITY_EDITOR || DEBUG_SPAWN_AREA
                        Logging.LogWarning(ToString(), $"Cannot spawn harvestable, it is collided to another entities, pending harvestable amount {_pending.Count}");
#endif
                        return true;
                    }
                }
            }
            else
            {
                int hitCount = Physics.OverlapSphereNonAlloc(position, entity.ColliderDetectionRadius, _overlaps3D);
                for (int i = 0; i < hitCount; ++i)
                {
                    Collider overlap = _overlaps3D[i];
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
#if UNITY_EDITOR || DEBUG_SPAWN_AREA
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
