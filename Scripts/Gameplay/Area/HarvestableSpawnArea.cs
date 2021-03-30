using System.Collections;
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

        private void Awake()
        {
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

        protected override HarvestableEntity SpawnInternal(HarvestableEntity prefab, short level)
        {
            Vector3 spawnPosition;
            if (GetRandomPosition(out spawnPosition))
            {
                Collider[] overlaps = Physics.OverlapSphere(spawnPosition, prefab.ColliderDetectionRadius);
                foreach (Collider overlap in overlaps)
                {
                    if (overlap.gameObject.layer == CurrentGameInstance.characterLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.itemDropLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.buildingLayer ||
                        overlap.gameObject.layer == CurrentGameInstance.harvestableLayer)
                    {
                        // Don't spawn because it will hitting other entities
                        pending.Add(new HarvestableSpawnPrefabData()
                        {
                            prefab = prefab,
                            level = level,
                            amount = 1
                        });
                        Logging.LogWarning(ToString(), $"Cannot spawn harvestable, it is collided to another entities, pending harvestable amount {pending.Count}");
                        return null;
                    }
                }
                Quaternion spawnRotation = GetRandomRotation();
                GameObject spawnObj = Instantiate(prefab.gameObject, spawnPosition, spawnRotation);
                HarvestableEntity entity = spawnObj.GetComponent<HarvestableEntity>();
                entity.SetSpawnArea(this, prefab, level, spawnPosition);
                BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
                return entity;
            }
            pending.Add(new HarvestableSpawnPrefabData()
            {
                prefab = prefab,
                level = level,
                amount = 1
            });
            Logging.LogWarning(ToString(), $"Cannot spawn harvestable, it cannot find grounded position, pending harvestable amount {pending.Count}");
            return null;
        }

        public override int GroundLayerMask
        {
            get { return CurrentGameInstance.GetHarvestableSpawnGroundDetectionLayerMask(); }
        }
    }
}
