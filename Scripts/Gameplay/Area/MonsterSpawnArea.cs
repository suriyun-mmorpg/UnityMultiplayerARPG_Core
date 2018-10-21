using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class MonsterSpawnArea : GameArea
    {
        [Header("Spawning Data")]
        [System.Obsolete("This will be deprecated on next version")]
        public MonsterCharacter database;
        public BaseMonsterCharacterEntity monsterCharacterEntity;
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

        public void RegisterAssets()
        {
            if (database != null)
                monsterCharacterEntity = database.entityPrefab as BaseMonsterCharacterEntity;
            if (monsterCharacterEntity != null)
                CacheGameNetworkManager.Assets.RegisterPrefab(monsterCharacterEntity.Identity);
        }

        public void SpawnAll()
        {
            if (monsterCharacterEntity != null)
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
            var spawnPosition = GetRandomPosition();
            var spawnRotation = GetRandomRotation();
            var identity = CacheGameNetworkManager.Assets.NetworkSpawn(monsterCharacterEntity.Identity, spawnPosition, spawnRotation);
            var entity = identity.GetComponent<BaseMonsterCharacterEntity>();
            entity.Level = level;
            entity.SetSpawnArea(this, spawnPosition);
        }

        public override int GroundLayerMask
        {
            get { return gameInstance.GetMonsterSpawnGroundDetectionLayerMask(); }
        }
    }
}
