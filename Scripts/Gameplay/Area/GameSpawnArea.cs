using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public abstract class GameSpawnArea<T> : GameArea where T : LiteNetLibBehaviour
    {
        [System.Serializable]
        public struct SpawnPrefabData
        {
            public T prefab;
            [Min(1)]
            public short level;
            [Min(1)]
            public short amount;
        }

        [Header("Spawning Data")]
        [FormerlySerializedAs("asset")]
        public T prefab;
        [Min(1)]
        public short level = 1;
        [Min(1)]
        public short amount = 1;
        public List<SpawnPrefabData> spawningPrefabs = new List<SpawnPrefabData>();
        public float respawnPendingEntitiesDelay = 5f;
        protected float respawnPendingEntitiesTimer = 0f;
        protected readonly List<SpawnPrefabData> pending = new List<SpawnPrefabData>();

        protected virtual void LateUpdate()
        {
            if (pending.Count > 0)
            {
                respawnPendingEntitiesTimer += Time.deltaTime;
                if (respawnPendingEntitiesTimer >= respawnPendingEntitiesDelay)
                {
                    respawnPendingEntitiesTimer = 0f;
                    foreach (SpawnPrefabData pendingEntry in pending)
                    {
                        Logging.LogWarning(ToString(), $"Spawning pending entities, Prefab: {pendingEntry.prefab.name}, Amount: {pendingEntry.amount}.");
                        for (int i = 0; i < pendingEntry.amount; ++i)
                        {
                            Spawn(pendingEntry.prefab, pendingEntry.level, 0).Forget();
                        }
                    }
                    pending.Clear();
                }
            }
        }

        public virtual void RegisterPrefabs()
        {
            if (prefab != null)
                BaseGameNetworkManager.Singleton.Assets.RegisterPrefab(prefab.Identity);
            foreach (SpawnPrefabData spawningPrefab in spawningPrefabs)
            {
                if (spawningPrefab.prefab != null)
                    BaseGameNetworkManager.Singleton.Assets.RegisterPrefab(spawningPrefab.prefab.Identity);
            }
        }

        public virtual void SpawnAll()
        {
            SpawnByAmount(prefab, level, amount);
            foreach (SpawnPrefabData spawningPrefab in spawningPrefabs)
            {
                SpawnByAmount(spawningPrefab.prefab, spawningPrefab.level, spawningPrefab.amount);
            }
        }

        public virtual void SpawnByAmount(T prefab, short level, int amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                Spawn(prefab, level, 0).Forget();
            }
        }

        public virtual async UniTaskVoid Spawn(T prefab, short level, float delay)
        {
            await UniTask.Delay(Mathf.CeilToInt(delay * 1000));
            SpawnInternal(prefab, level);
        }

        protected abstract T SpawnInternal(T prefab, short level);
    }
}
