using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct HarvestableSpawnAmount
    {
        public HarvestableEntity harvestableEntity;
        public int amount;
    }

    public class HarvestableSpawnArea : MonoBehaviour
    {
        public float randomRadius = 5f;
        public HarvestableSpawnAmount[] spawningHarvestables;

        private readonly List<string> pendingSpawningHarvestables = new List<string>();
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

        public void RegisterAssets()
        {
            if (CacheGameNetworkManager != null)
            {
                foreach (var spawningHarvestable in spawningHarvestables)
                {
                    if (spawningHarvestable.harvestableEntity == null || spawningHarvestable.amount <= 0)
                        continue;
                    
                    CacheGameNetworkManager.Assets.RegisterPrefab(spawningHarvestable.harvestableEntity.Identity);
                }
            }
        }

        public void SpawnAll()
        {
            foreach (var spawningHarvestable in spawningHarvestables)
            {
                if (spawningHarvestable.harvestableEntity == null || spawningHarvestable.amount <= 0)
                    continue;

                for (var i = 0; i < spawningHarvestable.amount; ++i)
                {
                    Spawn(spawningHarvestable.harvestableEntity.Identity.AssetId, 0);
                }
            }
        }

        public void Spawn(string assetId, float delay)
        {
            StartCoroutine(SpawnRoutine(assetId, delay));
        }

        IEnumerator SpawnRoutine(string assetId, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            var spawnPosition = GetRandomPosition();
            var spawnRotation = GetRandomRotation();
            var identity = CacheGameNetworkManager.Assets.NetworkSpawn(assetId, spawnPosition, spawnRotation);
            if (identity != null)
            {
                var entity = identity.GetComponent<HarvestableEntity>();
                entity.spawnArea = this;
                entity.spawnPosition = spawnPosition;
            }
        }

        public Vector3 GetRandomPosition()
        {
            var randomedPosition = Random.insideUnitSphere * randomRadius;
            randomedPosition = transform.position + new Vector3(randomedPosition.x, 0, randomedPosition.z);
            return randomedPosition;
        }

        public Quaternion GetRandomRotation()
        {
            var randomedRotation = Vector3.up * Random.Range(0, 360);
            return Quaternion.Euler(randomedRotation);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, randomRadius);
        }
    }
}
