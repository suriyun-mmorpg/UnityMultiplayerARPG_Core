using Cysharp.Threading.Tasks;
using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract class GameSpawnArea : GameArea
    {
        public const float SPAWN_UPDATE_DELAY = 1f;
        public enum SpawnType
        {
            Default,
            SpawnIfPlayerNearby,
        }

        [Header("Spawning Data")]
        [FormerlySerializedAs("level")]
        [Min(1)]
        public int minLevel = 1;
        [Min(1)]
        public int maxLevel = 1;
        [FormerlySerializedAs("amount")]
        [Min(1)]
        public int minAmount = 1;
        [Min(1)]
        public int maxAmount = 1;
        public float destroyRespawnDelay = 0f;
        public float respawnPendingEntitiesDelay = 5f;
        public SpawnType spawnType = SpawnType.Default;
        [Tooltip("If `spawnType` is `SpawnIfPlayerNearby`, and there is no players nearby this spawn area, it will destroy all spawned objects within `noPlayerNearbyDestroyDelay`")]
        public float noPlayerNearbyDestroyDelay = 60f;
        public float additionalRangeToFindNearbyPlayers = 30f;

        protected GameSpawnAreaSubscribeHandler _subscribeHandler;

        protected virtual void Awake()
        {
            _subscribeHandler = new GameSpawnAreaSubscribeHandler(this);
            SpatialObjectContainer.Add(_subscribeHandler);
            gameObject.layer = PhysicLayers.IgnoreRaycast;
        }

        protected virtual void OnDestroy()
        {
            SpatialObjectContainer.Remove(_subscribeHandler);
            _subscribeHandler?.Clean();
            _subscribeHandler = null;
        }

        protected virtual void LateUpdate()
        {
            if (!BaseGameNetworkManager.Singleton.IsServer)
                return;

            _subscribeHandler.SpatialObjectEnabled = spawnType == SpawnType.SpawnIfPlayerNearby;
            if (_subscribeHandler.SpatialObjectEnabled)
                _subscribeHandler.Update(Time.deltaTime, noPlayerNearbyDestroyDelay);
        }

        public int GetRandomedSpawnAmount()
        {
            if (maxAmount < minAmount)
                return minAmount;
            return Random.Range(minAmount, maxAmount);
        }

        public void SpawnFirstTime()
        {
            SpawnAll();
        }

        public bool AbleToSpawn()
        {
            if (spawnType == SpawnType.SpawnIfPlayerNearby && _subscribeHandler.CurrentSpawnState == GameSpawnAreaSubscribeHandler.SpawnState.Despawned)
            {
                // Unable to spawn yet
                return false;
            }
            return true;
        }

        public abstract void OnDestroyBySubscribeHandler(GameSpawnAreaEntityHandler handler);
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
            [FormerlySerializedAs("amount")]
            [Min(1)]
            public int minAmount = 1;
            [Min(1)]
            public int maxAmount = 1;
            public float destroyRespawnDelay;

            public int GetRandomedSpawnAmount()
            {
                if (maxAmount < minAmount)
                    return minAmount;
                return Random.Range(minAmount, maxAmount);
            }
        }

        [System.Serializable]
        public class SpawnPendingData
        {
#if !EXCLUDE_PREFAB_REFS
            public T prefab;
#endif
            public AddressablePrefab addressablePrefab;
            public int level;
            public float countdown;
            public float destroyRespawnDelay;
        }

        [Header("Spawning Prefab Data")]
#if !EXCLUDE_PREFAB_REFS
        [FormerlySerializedAs("asset")]
        public T prefab;
#endif
        public AddressablePrefab addressablePrefab;

        [Header("Multiple Spawning Data")]
        public List<SpawnPrefabData> spawningPrefabs = new List<SpawnPrefabData>();

        protected float secondsCounter = 0f;
        protected List<SpawnPendingData> _pending = new List<SpawnPendingData>();
        protected List<SpawnPendingData> _unableToSpawns = new List<SpawnPendingData>();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            spawningPrefabs?.Clear();
            spawningPrefabs = null;
            _pending?.Clear();
            _pending = null;
            _unableToSpawns?.Clear();
            _unableToSpawns = null;
        }

        protected override void LateUpdate()
        {
            if (!BaseGameNetworkManager.Singleton.IsServer)
                return;

            base.LateUpdate();

            if (!AbleToSpawn())
                return;

            float deltaTime = Time.deltaTime;
            if (_pending.Count > 0)
            {
                secondsCounter += deltaTime;
                if (secondsCounter < SPAWN_UPDATE_DELAY)
                    return;
                float decreaseCountdown = secondsCounter;
                secondsCounter -= SPAWN_UPDATE_DELAY;
                for (int i = _pending.Count - 1; i >= 0; --i)
                {
                    SpawnPendingData pendingEntry = _pending[i];
                    if (pendingEntry == null)
                    {
                        _pending.RemoveAt(i);
                        continue;
                    }

                    pendingEntry.countdown -= decreaseCountdown;
                    if (pendingEntry.countdown > 0f)
                    {
                        continue;
                    }
                    SpawnRoutine(i);
                }
            }

            if (_unableToSpawns.Count > 0)
            {
                // Add unable spawns to pending list
                for (int i = 0; i < _unableToSpawns.Count; ++i)
                {
                    _pending.Add(_unableToSpawns[i]);
                }
                _unableToSpawns.Clear();
            }
        }

        public virtual async void RegisterPrefabs()
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
                await BaseGameNetworkManager.Singleton.Assets.RegisterAddressablePrefabAsync(addressablePrefab);

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
                    await BaseGameNetworkManager.Singleton.Assets.RegisterAddressablePrefabAsync(spawningPrefab.addressablePrefab);
            }
        }

        public override void OnDestroyBySubscribeHandler(GameSpawnAreaEntityHandler handler)
        {
            SpawnPendingData pendingData = handler.SpawnData as SpawnPendingData;
            pendingData.countdown = 1f;
            AddPending(pendingData);
        }

#if UNITY_EDITOR
        [ContextMenu("Spawn All")]
#endif
        public override void SpawnAll()
        {
            T prefab = null;
#if !EXCLUDE_PREFAB_REFS
            prefab = this.prefab;
#endif
            AddressablePrefab addressablePrefab = this.addressablePrefab;
            if (prefab != null || addressablePrefab.IsDataValid())
            {
                SpawnByAmount(prefab, addressablePrefab, Random.Range(minLevel, maxLevel + 1), GetRandomedSpawnAmount(), destroyRespawnDelay);
            }
            foreach (SpawnPrefabData spawningPrefab in spawningPrefabs)
            {
                prefab = null;
#if !EXCLUDE_PREFAB_REFS
                prefab = spawningPrefab.prefab;
#endif
                addressablePrefab = spawningPrefab.addressablePrefab;
                float destroyRespawnDelay = spawningPrefab.destroyRespawnDelay;
                if (destroyRespawnDelay <= 0f)
                    destroyRespawnDelay = this.destroyRespawnDelay;
                SpawnByAmount(prefab, addressablePrefab, spawningPrefab.level, spawningPrefab.GetRandomedSpawnAmount(), destroyRespawnDelay);
            }
        }

        public virtual void SpawnByAmount(T prefab, AddressablePrefab addressablePrefab, int level, int amount, float destroyRespawnDelay)
        {
            for (int i = 0; i < amount; ++i)
            {
                Spawn(prefab, addressablePrefab, level, 0, destroyRespawnDelay);
            }
        }

        public virtual void Spawn(T prefab, AddressablePrefab addressablePrefab, int level, float delay, float destroyRespawnDelay)
        {
            if (prefab == null && !addressablePrefab.IsDataValid())
                return;

            AddPending(new SpawnPendingData()
            {
                prefab = prefab,
                addressablePrefab = addressablePrefab,
                level = level,
                countdown = delay,
                destroyRespawnDelay = destroyRespawnDelay,
            });
        }

        private void SpawnRoutine(int pendingIndex)
        {
            if (!AbleToSpawn())
            {
                Debug.Log($"Not able to spawn, Spawn Type={spawnType}, Spawn State={_subscribeHandler.CurrentSpawnState}");
                return;
            }
            SpawnPendingData pendingEntry = _pending[pendingIndex];
            _pending.RemoveAt(pendingIndex);

            T prefab = null;
#if !EXCLUDE_PREFAB_REFS
            prefab = pendingEntry.prefab;
#endif
            AddressablePrefab addressablePrefab = pendingEntry.addressablePrefab;

            int level = pendingEntry.level <= 0 ? 1 : pendingEntry.level;
            float destroyRespawnDelay = pendingEntry.destroyRespawnDelay;
            if (destroyRespawnDelay <= 0f)
                destroyRespawnDelay = this.destroyRespawnDelay;

            T newEntity = SpawnInternal(prefab, addressablePrefab, level, destroyRespawnDelay);
            if (newEntity != null)
            {
                // Store to entities collection, so the spawner can manage them later
                _subscribeHandler.AddEntity(newEntity, pendingEntry);
            }
            else
            {
                // Unable to spawn yet, will add to spawn pending later
                pendingEntry.countdown = respawnPendingEntitiesDelay;
                _unableToSpawns.Add(pendingEntry);
            }
        }

        protected abstract T SpawnInternal(T prefab, AddressablePrefab addressablePrefab, int level, float destroyRespawnDelay);

        protected virtual void AddPending(SpawnPendingData data)
        {
            _pending.Add(data);
        }

        public virtual void CountSpawningObjects()
        {
            int count = 0;
            GameSpawnArea<T>[] areas = FindObjectsOfType<GameSpawnArea<T>>();
            foreach (GameSpawnArea<T> area in areas)
            {
                count += area.minAmount;
                List<SpawnPrefabData> spawningPrefabs = new List<SpawnPrefabData>(area.spawningPrefabs);
                foreach (SpawnPrefabData spawningPrefab in spawningPrefabs)
                {
                    count += spawningPrefab.minAmount;
                }
            }
            Debug.Log($"Spawning {typeof(T).Name} Amount: {count}");
        }
    }
}
