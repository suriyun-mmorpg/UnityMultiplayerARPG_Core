using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class MonsterSpawnArea : GameArea
    {
        [Header("Spawning Data")]
        public BaseMonsterCharacterEntity monsterCharacterEntity;
        public short level = 1;
        public short amount = 1;
        
        private GameInstance gameInstance { get { return GameInstance.Singleton; } }

        public void RegisterAssets()
        {
            if (monsterCharacterEntity != null)
                BaseGameNetworkManager.Singleton.Assets.RegisterPrefab(monsterCharacterEntity.Identity);
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
            var identity = BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(monsterCharacterEntity.Identity, spawnPosition, spawnRotation);
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
