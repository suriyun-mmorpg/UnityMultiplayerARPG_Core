using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class HarvestableSpawnArea : MonoBehaviour
    {
        public const float GROUND_DETECTION_DISTANCE = 100f;
        public HarvestableEntity harvestableEntity;
        public short amount = 1;
        public float randomRadius = 5f;

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
                    entity.spawnArea = this;
                    entity.spawnPosition = spawnPosition;
                    entity.CurrentHp = harvestableEntity.maxHp;
                }
            }
            else
            {
                ++pending;
                Debug.LogWarning("[HarvestableSpawnArea(" + name + ")] Cannot spawn harvestable it is collided to another entities, pending harvestable amount " + pending);
            }
        }

        public Vector3 GetRandomPosition()
        {
            var randomedPosition = Random.insideUnitSphere * randomRadius;
            randomedPosition = transform.position + new Vector3(randomedPosition.x, 0, randomedPosition.z);

            // Raycast to find hit floor
            Vector3? aboveHitPoint = null;
            Vector3? underHitPoint = null;
            var raycastLayerMask = gameInstance.GetHarvestableSpawnGroundDetectionLayerMask();
            RaycastHit tempHit;
            if (Physics.Raycast(randomedPosition, Vector3.up, out tempHit, GROUND_DETECTION_DISTANCE, raycastLayerMask))
                aboveHitPoint = tempHit.point;
            if (Physics.Raycast(randomedPosition, Vector3.down, out tempHit, GROUND_DETECTION_DISTANCE, raycastLayerMask))
                underHitPoint = tempHit.point;
            // Set drop position to nearest hit point
            if (aboveHitPoint.HasValue && underHitPoint.HasValue)
            {
                if (Vector3.Distance(randomedPosition, aboveHitPoint.Value) < Vector3.Distance(randomedPosition, underHitPoint.Value))
                    randomedPosition = aboveHitPoint.Value;
                else
                    randomedPosition = underHitPoint.Value;
            }
            else if (aboveHitPoint.HasValue)
                randomedPosition = aboveHitPoint.Value;
            else if (underHitPoint.HasValue)
                randomedPosition = underHitPoint.Value;

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
