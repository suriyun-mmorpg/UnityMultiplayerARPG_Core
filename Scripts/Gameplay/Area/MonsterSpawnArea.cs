using System.Collections;
using UnityEngine;
using LiteNetLibManager;
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
            if (prefab == null && monsterCharacterEntity != null)
            {
                prefab = monsterCharacterEntity;
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

        protected override BaseMonsterCharacterEntity SpawnInternal(BaseMonsterCharacterEntity prefab, short level)
        {
            Vector3 spawnPosition = GetRandomPosition();
            Quaternion spawnRotation = GetRandomRotation();
            GameObject spawnObj = Instantiate(prefab.gameObject, spawnPosition, spawnRotation);
            BaseMonsterCharacterEntity entity = spawnObj.GetComponent<BaseMonsterCharacterEntity>();
            entity.gameObject.SetActive(false);
            if (entity.FindGroundedPosition(spawnPosition, GROUND_DETECTION_DISTANCE, out spawnPosition))
            {
                entity.Level = level;
                entity.SetSpawnArea(this, prefab, level, spawnPosition);
                BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
                return entity;
            }
            // Destroy the entity (because it can't find ground position)
            Destroy(entity.gameObject);
            pending.Add(new SpawnPrefabData()
            {
                prefab = prefab,
                level = level,
                amount = 1
            });
            Logging.LogWarning(ToString(), $"Cannot spawn monster, it cannot find grounded position, pending monster amount {pending.Count}");
            return null;
        }

        public override int GroundLayerMask
        {
            get { return CurrentGameInstance.GetMonsterSpawnGroundDetectionLayerMask(); }
        }
    }
}
