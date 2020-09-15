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
        public short level = 1;

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
            if (asset == null && monsterCharacterEntity != null)
            {
                asset = monsterCharacterEntity;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public override void RegisterAssets()
        {
            base.RegisterAssets();
            GameInstance.AddCharacterEntities(asset);
        }

        protected override void SpawnInternal()
        {
            Vector3 spawnPosition = GetRandomPosition();
            Quaternion spawnRotation = GetRandomRotation();
            GameObject spawnObj = Instantiate(asset.gameObject, spawnPosition, spawnRotation);
            BaseMonsterCharacterEntity entity = spawnObj.GetComponent<BaseMonsterCharacterEntity>();
            entity.gameObject.SetActive(false);
            if (entity.FindGroundedPosition(spawnPosition, GROUND_DETECTION_DISTANCE, out spawnPosition))
            {
                entity.Level = level;
                entity.SetSpawnArea(this, spawnPosition);
                BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            }
            else
            {
                // Destroy the entity (because it can't find ground position)
                Destroy(entity.gameObject);
                ++pending;
                Logging.LogWarning(ToString(), "Cannot spawn monster, it cannot find grounded position, pending monster amount " + pending);
            }
        }

        public override int GroundLayerMask
        {
            get { return CurrentGameInstance.GetMonsterSpawnGroundDetectionLayerMask(); }
        }
    }
}
