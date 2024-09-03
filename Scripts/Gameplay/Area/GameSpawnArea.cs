using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public abstract class GameSpawnArea : GameArea
    {
        protected virtual void Awake()
        {
            gameObject.layer = PhysicLayers.IgnoreRaycast;
        }

        public abstract void SpawnAll();
    }

    public abstract class GameSpawnArea<T> : GameSpawnArea where T : LiteNetLibBehaviour
    {
        [System.Serializable]
        public class AddressablePrefab : AssetReferenceLiteNetLibBehaviour<T>
        {
            public AddressablePrefab(string guid) : base(guid)
            {
            }

#if UNITY_EDITOR
            public AddressablePrefab(T behaviour) : base(behaviour)
            {
            }
#endif
        }

        [System.Serializable]
        public class SpawnPrefabData
        {
#if !EXCLUDE_PREFAB_REFS
            public T prefab;
#endif
            public AddressablePrefab addressablePrefab;
            [Min(1)]
            public int level;
            [Min(1)]
            public int amount;
        }

        [Header("Spawning Data")]
#if !EXCLUDE_PREFAB_REFS
        [FormerlySerializedAs("asset")]
        public T prefab;
#endif
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

        protected virtual void LateUpdate()
        {
            if (_pending.Count > 0)
            {
                _respawnPendingEntitiesTimer += Time.deltaTime;
                if (_respawnPendingEntitiesTimer >= respawnPendingEntitiesDelay)
                {
                    _respawnPendingEntitiesTimer = 0f;
                    T prefab;
                    AddressablePrefab addressablePrefab;
                    foreach (SpawnPrefabData pendingEntry in _pending)
                    {
                        prefab = null;
#if !EXCLUDE_PREFAB_REFS
                        prefab = pendingEntry.prefab;
#endif
                        addressablePrefab = pendingEntry.addressablePrefab;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Logging.LogWarning(ToString(), $"Spawning pending entities, Prefab: {prefab?.name ?? "None"}, Addressable: {addressablePrefab?.RuntimeKey ?? "None"}, Amount: {pendingEntry.amount}.");
#endif
                        for (int i = 0; i < pendingEntry.amount; ++i)
                        {
                            Spawn(prefab, addressablePrefab, pendingEntry.level, 0);
                        }
                    }
                    _pending.Clear();
                }
            }
        }

        public virtual void RegisterPrefabs()
        {
            T prefab;
#if !EXCLUDE_PREFAB_REFS
            prefab = this.prefab;
#else
            prefab = null;
#endif
            if (prefab != null)
                BaseGameNetworkManager.Singleton.Assets.RegisterPrefab(prefab.Identity);
            else if (addressablePrefab.IsDataValid())
                BaseGameNetworkManager.Singleton.Assets.RegisterAddressablePrefab(addressablePrefab);

            foreach (SpawnPrefabData spawningPrefab in spawningPrefabs)
            {
#if !EXCLUDE_PREFAB_REFS
                prefab = spawningPrefab.prefab;
#else
                prefab = null;
#endif
                if (prefab != null)
                    BaseGameNetworkManager.Singleton.Assets.RegisterPrefab(prefab.Identity);
                else if (spawningPrefab.addressablePrefab.IsDataValid())
                    BaseGameNetworkManager.Singleton.Assets.RegisterAddressablePrefab(spawningPrefab.addressablePrefab);
            }
        }

        public override void SpawnAll()
        {
            T prefab = null;
#if !EXCLUDE_PREFAB_REFS
            prefab = this.prefab;
#endif
            AddressablePrefab addressablePrefab = this.addressablePrefab;
            if (prefab != null || addressablePrefab.IsDataValid())
            {
                for (int i = 0; i < amount; ++i)
                {
                    Spawn(prefab, addressablePrefab, Random.Range(minLevel, maxLevel + 1), 0);
                }
            }
            foreach (SpawnPrefabData spawningPrefab in spawningPrefabs)
            {
                prefab = null;
#if !EXCLUDE_PREFAB_REFS
                prefab = spawningPrefab.prefab;
#endif
                addressablePrefab = spawningPrefab.addressablePrefab;
                SpawnByAmount(prefab, addressablePrefab, spawningPrefab.level, spawningPrefab.amount);
            }
        }

        public virtual void SpawnByAmount(T prefab, AddressablePrefab addressablePrefab, int level, int amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                Spawn(prefab, addressablePrefab, level, 0);
            }
        }

        public virtual Coroutine Spawn(T prefab, AddressablePrefab addressablePrefab, int level, float delay)
        {
            return StartCoroutine(SpawnRoutine(prefab, addressablePrefab, level, delay));
        }

        IEnumerator SpawnRoutine(T prefab, AddressablePrefab addressablePrefab, int level, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            T newEntity = SpawnInternal(prefab, addressablePrefab, level);
            if (newEntity == null)
            {
                AddPending(new SpawnPrefabData()
                {
#if !EXCLUDE_PREFAB_REFS
                    prefab = prefab,
#endif
                    addressablePrefab = addressablePrefab,
                    level = level,
                    amount = 1,
                });
            }
        }

        protected abstract T SpawnInternal(T prefab, AddressablePrefab addressablePrefab, int level);

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
