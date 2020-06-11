using System.Collections;
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
                monsterCharacterEntity = null;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public override void RegisterAssets()
        {
            base.RegisterAssets();
            GameInstance.AddCharacterEntities(new BaseMonsterCharacterEntity[] { asset });
        }

        protected override void SpawnInternal()
        {
            Vector3 spawnPosition = GetRandomPosition();
            Quaternion spawnRotation = GetRandomRotation();
            GameObject spawnObj = Instantiate(asset.gameObject, spawnPosition, spawnRotation);
            BaseMonsterCharacterEntity entity = spawnObj.GetComponent<BaseMonsterCharacterEntity>();
            entity.Level = level;
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            entity.SetSpawnArea(this, spawnPosition);
        }

        public override int GroundLayerMask
        {
            get { return CurrentGameInstance.GetMonsterSpawnGroundDetectionLayerMask(); }
        }
    }
}
