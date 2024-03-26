using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public abstract class GameSpawnArea<T> : GameArea where T : LiteNetLibBehaviour
    {
        [System.Serializable]
        public class AddressablePrefab : ComponentReference<T>
        {
            public AddressablePrefab(string guid) : base(guid)
            {
            }
        }

        [System.Serializable]
        public class SpawnPrefabData
        {
            public T prefab;
            public AddressablePrefab addressablePrefab;
            [Min(1)]
            public int level;
            [Min(1)]
            public int amount;
        }

        [Header("Spawning Data")]
        [FormerlySerializedAs("asset")]
        public T prefab;
        public AddressablePrefab addressablePrefab;
        [FormerlySerializedAs("level")]
        [Min(1)]
        public int minLevel = 1;
        [Min(1)]
        public int maxLevel = 1;
        [Min(1)]
        public int amount = 1;
        public List<SpawnPrefabData> spawningPrefabs = new List<SpawnPrefabData>();
        public float respawnPendingEntitiesDelay = 5f;

        protected float _respawnPendingEntitiesTimer = 0f;
        protected readonly List<SpawnPrefabData> _pending = new List<SpawnPrefabData>();

        protected virtual void Awake()
        {
            gameObject.layer = PhysicLayers.IgnoreRaycast;
        }

        protected virtual void LateUpdate()
        {
            if (_pending.Count > 0)
            {
                _respawnPendingEntitiesTimer += Time.deltaTime;
                if (_respawnPendingEntitiesTimer >= respawnPendingEntitiesDelay)
                {
                    _respawnPendingEntitiesTimer = 0f;
                    foreach (SpawnPrefabData pendingEntry in _pending)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Logging.LogWarning(ToString(), $"Spawning pending entities, Prefab: {pendingEntry.prefab.name}, Amount: {pendingEntry.amount}.");
#endif
                        for (int i = 0; i < pendingEntry.amount; ++i)
                        {
                            Spawn(pendingEntry.prefab, pendingEntry.level, 0);
                        }
                    }
                    _pending.Clear();
                }
            }
        }

        public virtual void RegisterPrefabs()
        {
#if !LNLM_NO_PREFABS
            if (prefab != null)
                BaseGameNetworkManager.Singleton.Assets.RegisterPrefab(prefab.Identity);
            foreach (SpawnPrefabData spawningPrefab in spawningPrefabs)
            {
                if (spawningPrefab.prefab != null)
                    BaseGameNetworkManager.Singleton.Assets.RegisterPrefab(spawningPrefab.prefab.Identity);
            }
#endif
        }

        public virtual void SpawnAll()
        {
            for (int i = 0; i < amount; ++i)
            {
                Spawn(prefab, Random.Range(minLevel, maxLevel + 1), 0);
            }
            foreach (SpawnPrefabData spawningPrefab in spawningPrefabs)
            {
                SpawnByAmount(spawningPrefab.prefab, spawningPrefab.level, spawningPrefab.amount);
            }
        }

        public virtual void SpawnByAmount(T prefab, int level, int amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                Spawn(prefab, level, 0);
            }
        }

        public virtual Coroutine Spawn(T prefab, int level, float delay)
        {
            return StartCoroutine(SpawnRoutine(prefab, level, delay));
        }

        IEnumerator SpawnRoutine(T prefab, int level, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            T newEntity = SpawnInternal(prefab, level);
            if (newEntity == null)
            {
                AddPending(new SpawnPrefabData()
                {
                    prefab = prefab,
                    level = level,
                    amount = 1,
                });
            }
        }

        protected abstract T SpawnInternal(T prefab, int level);

        protected virtual void AddPending(SpawnPrefabData data)
        {
            _pending.Add(data);
        }

        public virtual void CountSpawningObjects()
        {
            int count = 0;
            GameSpawnArea<T>[] areas = FindObjectsOfType<GameSpawnArea<T>>();
            foreach (GameSpawnArea<T> area in areas)
            {
                count += area.amount;
                List<SpawnPrefabData> spawningPrefabs = new List<SpawnPrefabData>(area.spawningPrefabs);
                foreach (SpawnPrefabData spawningPrefab in spawningPrefabs)
                {
                    count += spawningPrefab.amount;
                }
            }
            Debug.Log($"Spawning {typeof(T).Name} Amount: {count}");
        }
    }
}
