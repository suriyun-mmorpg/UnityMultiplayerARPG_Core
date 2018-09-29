using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class HarvestableSpawnArea : GameArea
    {
        [Header("Spawning Data")]
        public HarvestableEntity harvestableEntity;
        public short amount = 1;

        private int pending;

        private BaseGameNetworkManager cacheGameNetworkManager;
        public BaseGameNetworkManager CacheGameNetworkManager
        {
            get
            {
                if (cacheGameNetworkManager == null)
                    cacheGameNetworkManager = FindObjectOfType<BaseGameNetworkManager>();
                if (cacheGameNetworkManager == null)
                    Debug.LogWarning("[HarvestableSpawnArea(" + name + ")] Cannot find `BaseGameNetworkManager`");
                return cacheGameNetworkManager;
            }
        }

        private GameInstance gameInstance { get { return GameInstance.Singleton; } }

        public void RegisterAssets()
        {
            if (harvestableEntity != null)
                CacheGameNetworkManager.Assets.RegisterPrefab(harvestableEntity.Identity);
        }

        public void SpawnAll()
        {
            if (harvestableEntity != null)
            {
                for (var i = 0; i < amount; ++i)
                {
                    Spawn(0);
                }
            }
        }

        public void Spawn(float delay)
        {
            StartCoroutine(SpawnRoutine(delay));
        }

        IEnumerator SpawnRoutine(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            var colliderDetectionRadius = harvestableEntity.colliderDetectionRadius;
            var spawnPosition = GetRandomPosition();
            var spawnRotation = GetRandomRotation();
            var overlapEntities = false;
            var overlaps = Physics.OverlapSphere(spawnPosition, colliderDetectionRadius);
            foreach (var overlap in overlaps)
            {
                if (overlap.gameObject.layer == gameInstance.characterLayer ||
                    overlap.gameObject.layer == gameInstance.itemDropLayer ||
                    overlap.gameObject.layer == gameInstance.buildingLayer ||
                    overlap.gameObject.layer == gameInstance.harvestableLayer)
                {
                    overlapEntities = true;
                    break;
                }
            }
            if (!overlapEntities)
            {
                var identity = CacheGameNetworkManager.Assets.NetworkSpawn(harvestableEntity.Identity, spawnPosition, spawnRotation);
                if (identity != null)
                {
                    var entity = identity.GetComponent<HarvestableEntity>();
                    entity.SetSpawnArea(this, spawnPosition);
                }
            }
            else
            {
                ++pending;
                Debug.LogWarning("[HarvestableSpawnArea(" + name + ")] Cannot spawn harvestable it is collided to another entities, pending harvestable amount " + pending);
            }
        }

        public override int GroundLayerMask
        {
            get { return gameInstance.GetHarvestableSpawnGroundDetectionLayerMask(); }
        }
    }
}
