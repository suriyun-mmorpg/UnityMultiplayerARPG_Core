using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class HarvestableSpawnArea : GameSpawnArea<HarvestableEntity>
    {
        [System.Serializable]
        public class HarvestableSpawnPrefabData : SpawnPrefabData<HarvestableEntity> { }

        public List<HarvestableSpawnPrefabData> spawningPrefabs = new List<HarvestableSpawnPrefabData>();
        public override SpawnPrefabData<HarvestableEntity>[] SpawningPrefabs
        {
            get { return spawningPrefabs.ToArray(); }
        }

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
        }

        public override void RegisterPrefabs()
        {
            base.RegisterPrefabs();
            GameInstance.AddHarvestableEntities(prefab);
        }

        protected override HarvestableEntity SpawnInternal(HarvestableEntity prefab, int level)
        {
            Vector3 spawnPosition;
            if (GetRandomPosition(out spawnPosition))
            {
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                {
                    Collider2D[] overlaps = Physics2D.OverlapCircleAll(spawnPosition, prefab.ColliderDetectionRadius);
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
                            _pending.Add(new HarvestableSpawnPrefabData()
                            {
                                prefab = prefab,
                                level = level,
                                amount = 1
                            });
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                            Logging.LogWarning(ToString(), $"Cannot spawn harvestable, it is collided to another entities, pending harvestable amount {_pending.Count}");
#endif
                            return null;
                        }
                    }
                }
                else
                {
                    Collider[] overlaps = Physics.OverlapSphere(spawnPosition, prefab.ColliderDetectionRadius);
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
                            _pending.Add(new HarvestableSpawnPrefabData()
                            {
                                prefab = prefab,
                                level = level,
                                amount = 1
                            });
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                            Logging.LogWarning(ToString(), $"Cannot spawn harvestable, it is collided to another entities, pending harvestable amount {_pending.Count}");
#endif
                            return null;
                        }
                    }
                }

                Quaternion spawnRotation = GetRandomRotation();
                LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    prefab.Identity.HashAssetId,
                    spawnPosition, spawnRotation);
                HarvestableEntity entity = spawnObj.GetComponent<HarvestableEntity>();
                entity.SetSpawnArea(this, prefab, level, spawnPosition);
                BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
                return entity;
            }
            _pending.Add(new HarvestableSpawnPrefabData()
            {
                prefab = prefab,
                level = level,
                amount = 1
            });
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Logging.LogWarning(ToString(), $"Cannot spawn harvestable, it cannot find grounded position, pending harvestable amount {_pending.Count}");
#endif
            return null;
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
