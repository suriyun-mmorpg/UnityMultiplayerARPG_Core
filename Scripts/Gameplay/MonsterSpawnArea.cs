using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class MonsterSpawnArea : MonoBehaviour
    {
        public const float GROUND_DETECTION_DISTANCE = 100f;
        public MonsterCharacter database;
        public short level = 1;
        public short amount = 1;
        public float randomRadius = 5f;
        
        private BaseGameNetworkManager cacheGameNetworkManager;
        public BaseGameNetworkManager CacheGameNetworkManager
        {
            get
            {
                if (cacheGameNetworkManager == null)
                    cacheGameNetworkManager = FindObjectOfType<BaseGameNetworkManager>();
                if (cacheGameNetworkManager == null)
                    Debug.LogWarning("[MonsterSpawnArea(" + name + ")] Cannot find `BaseGameNetworkManager`");
                return cacheGameNetworkManager;
            }
        }
        private GameInstance gameInstance { get { return GameInstance.Singleton; } }
        private int dataId { get { return database == null ? 0 : database.DataId; } }

        public void SpawnAll()
        {
            if (database == null)
            {
                Debug.LogWarning("Have to set monster database to spawn monster");
                return;
            }
            MonsterCharacter foundDatabase;
            if (!GameInstance.MonsterCharacters.TryGetValue(dataId, out foundDatabase))
            {
                Debug.LogWarning("The monster database have to be added to game instance");
                return;
            }
            for (var i = 0; i < amount; ++i)
            {
                Spawn(0);
            }
        }

        public void Spawn(float delay)
        {
            StartCoroutine(SpawnRoutine(delay));
        }

        IEnumerator SpawnRoutine(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            var spawnPosition = GetRandomPosition();
            var spawnRotation = GetRandomRotation();
            var monsterCharacterPrefab = database.entityPrefab;
            if (monsterCharacterPrefab != null)
            {
                var identity = CacheGameNetworkManager.Assets.NetworkSpawn(monsterCharacterPrefab.Identity, spawnPosition, spawnRotation);
                var entity = identity.GetComponent<BaseMonsterCharacterEntity>();
                entity.Id = string.Empty;
                entity.DataId = dataId;
                entity.Level = level;
                var stats = entity.GetStats();
                entity.CurrentHp = (int)stats.hp;
                entity.CurrentMp = (int)stats.mp;
                entity.CurrentStamina = (int)stats.stamina;
                entity.CurrentFood = (int)stats.food;
                entity.CurrentWater = (int)stats.water;
                entity.spawnArea = this;
                entity.spawnPosition = spawnPosition;
            }
        }

        public Vector3 GetRandomPosition()
        {
            var randomedPosition = Random.insideUnitSphere * randomRadius;
            randomedPosition = transform.position + new Vector3(randomedPosition.x, 0, randomedPosition.z);

            // Raycast to find hit floor
            Vector3? aboveHitPoint = null;
            Vector3? underHitPoint = null;
            var raycastLayerMask = gameInstance.GetMonsterSpawnGroundDetectionLayerMask();
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
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, randomRadius);
        }
    }
}
