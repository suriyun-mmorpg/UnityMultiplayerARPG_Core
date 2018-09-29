using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class MonsterSpawnArea : GameArea
    {
        [Header("Spawning Data")]
        public MonsterCharacter database;
        public short level = 1;
        public short amount = 1;
        
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
                entity.DataId = dataId;
                entity.Level = level;
                entity.SetSpawnArea(this, spawnPosition);
            }
        }

        public override int GroundLayerMask
        {
            get { return gameInstance.GetMonsterSpawnGroundDetectionLayerMask(); }
        }
    }
}
