using System.Collections;
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
        // Private data
        private int pending;

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
            if (asset == null && harvestableEntity != null)
            {
                asset = harvestableEntity;
                harvestableEntity = null;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public override void RegisterAssets()
        {
            base.RegisterAssets();
            GameInstance.AddHarvestableEntities(new HarvestableEntity[] { asset });
        }

        protected override void SpawnInternal()
        {
            float colliderDetectionRadius = harvestableEntity.colliderDetectionRadius;
            Vector3 spawnPosition = GetRandomPosition();
            Quaternion spawnRotation = GetRandomRotation();
            bool overlapEntities = false;
            Collider[] overlaps = Physics.OverlapSphere(spawnPosition, colliderDetectionRadius);
            foreach (Collider overlap in overlaps)
            {
                if (overlap.gameObject.layer == CurrentGameInstance.characterLayer ||
                    overlap.gameObject.layer == CurrentGameInstance.itemDropLayer ||
                    overlap.gameObject.layer == CurrentGameInstance.buildingLayer ||
                    overlap.gameObject.layer == CurrentGameInstance.harvestableLayer)
                {
                    overlapEntities = true;
                    break;
                }
            }
            if (!overlapEntities)
            {
                GameObject spawnObj = Instantiate(harvestableEntity.gameObject, spawnPosition, spawnRotation);
                HarvestableEntity entity = spawnObj.GetComponent<HarvestableEntity>();
                entity.SetSpawnArea(this, spawnPosition);
                BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            }
            else
            {
                ++pending;
                Logging.LogWarning(ToString(), "Cannot spawn harvestable it is collided to another entities, pending harvestable amount " + pending);
            }
        }

        public override int GroundLayerMask
        {
            get { return CurrentGameInstance.GetHarvestableSpawnGroundDetectionLayerMask(); }
        }
    }
}
