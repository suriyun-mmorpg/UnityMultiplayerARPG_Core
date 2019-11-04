using System.Collections;
using UnityEngine;

namespace MultiplayerARPG
{
    public class MonsterSpawnArea : GameArea
    {
        [Header("Spawning Data")]
        public BaseMonsterCharacterEntity monsterCharacterEntity;
        public short level = 1;
        public short amount = 1;

        public void RegisterAssets()
        {
            if (monsterCharacterEntity != null)
                BaseGameNetworkManager.Singleton.Assets.RegisterPrefab(monsterCharacterEntity.Identity);
        }

        public virtual void SpawnAll()
        {
            if (monsterCharacterEntity != null)
            {
                for (int i = 0; i < amount; ++i)
                {
                    Spawn(0);
                }
            }
        }

        public virtual void Spawn(float delay)
        {
            StartCoroutine(SpawnRoutine(delay));
        }

        IEnumerator SpawnRoutine(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            Vector3 spawnPosition = GetRandomPosition();
            Quaternion spawnRotation = GetRandomRotation();
            GameObject spawnObj = Instantiate(monsterCharacterEntity.gameObject, spawnPosition, spawnRotation);
            BaseMonsterCharacterEntity entity = spawnObj.GetComponent<BaseMonsterCharacterEntity>();
            entity.Level = level;
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            entity.SetSpawnArea(this, spawnPosition);
        }

        public override int GroundLayerMask
        {
            get { return gameInstance.GetMonsterSpawnGroundDetectionLayerMask(); }
        }
    }
}
