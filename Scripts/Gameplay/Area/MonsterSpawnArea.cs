using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class MonsterSpawnArea : GameSpawnArea<BaseMonsterCharacterEntity>
    {
        [Tooltip("This is deprecated, might be removed in future version, set your asset to `Asset` instead.")]
        [ReadOnlyField]
        public BaseMonsterCharacterEntity monsterCharacterEntity;
        public Faction faction;

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
            if (prefab == null && monsterCharacterEntity != null)
            {
                prefab = monsterCharacterEntity;
                monsterCharacterEntity = null;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
            if (prefab != null && monsterCharacterEntity != null)
            {
                monsterCharacterEntity = null;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public override void RegisterPrefabs()
        {
            base.RegisterPrefabs();
            GameInstance.AddCharacterEntities(prefab);
        }

        protected override BaseMonsterCharacterEntity SpawnInternal(BaseMonsterCharacterEntity prefab, AddressablePrefab addressablePrefab, int level)
        {
            if (!GetRandomPosition(out Vector3 spawnPosition))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Logging.LogWarning(ToString(), $"Cannot spawn monster, it cannot find grounded position, pending monster amount {_pending.Count}");
#endif
                return null;
            }

            Quaternion spawnRotation = GetRandomRotation();
            LiteNetLibIdentity spawnObj = null;
            BaseMonsterCharacterEntity entity = null;
            if (prefab != null)
            {
                spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    prefab.Identity.HashAssetId,
                    spawnPosition, spawnRotation);
                if (spawnObj == null)
                    return null;
                entity = spawnObj.GetComponent<BaseMonsterCharacterEntity>();
                entity.SetSpawnArea(this, prefab, level, spawnPosition);
            }
            else if (addressablePrefab.IsDataValid())
            {
                spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    addressablePrefab.HashAssetId,
                    spawnPosition, spawnRotation);
                if (spawnObj == null)
                    return null;
                entity = spawnObj.GetComponent<BaseMonsterCharacterEntity>();
                entity.SetSpawnArea(this, addressablePrefab, level, spawnPosition);
            }

            if (entity == null)
            {
                Logging.LogWarning(ToString(), $"Cannot spawn monster, entity is null");
                return null;
            }

            if (!entity.FindGroundedPosition(spawnPosition, GROUND_DETECTION_DISTANCE, out spawnPosition))
            {
                // Destroy the entity (because it can't find ground position)
                BaseGameNetworkManager.Singleton.Assets.DestroyObjectInstance(spawnObj);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Logging.LogWarning(ToString(), $"Cannot spawn monster, it cannot find grounded position, pending monster amount {_pending.Count}");
#endif
                return null;
            }

            entity.Level = level;
            entity.Faction = faction;
            entity.Teleport(spawnPosition, spawnRotation, false);
            entity.InitStats();
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            return entity;
        }

        public override int GroundLayerMask
        {
            get { return CurrentGameInstance.GetGameEntityGroundDetectionLayerMask(); }
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
