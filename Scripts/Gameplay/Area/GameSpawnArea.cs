using Cysharp.Threading.Tasks;
using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract class GameSpawnArea : GameArea
    {
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
            if (spawnType == SpawnType.SpawnIfPlayerNearby)
                return;
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

        public abstract void SpawnAll();
        public abstract void CancelAllSpawning();
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

        protected float _respawnPendingEntitiesTimer = 0f;
        protected List<SpawnPrefabData> _pending = new List<SpawnPrefabData>();
        protected List<CancellationTokenSource> _spawnCancellations = new List<CancellationTokenSource>();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            spawningPrefabs?.Clear();
            spawningPrefabs = null;
            _pending?.Clear();
            _pending = null;
            CancelAllSpawning();
            _spawnCancellations = null;
        }

        protected override void LateUpdate()
        {
            if (!BaseGameNetworkManager.Singleton.IsServer)
                return;

            base.LateUpdate();

            if (!AbleToSpawn())
                return;

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
#if UNITY_EDITOR || DEBUG_SPAWN_AREA
                        Logging.LogWarning(ToString(), $"Spawning pending entities, Prefab: {prefab?.name ?? "None"}, Addressable: {addressablePrefab?.RuntimeKey ?? "None"}, Amount: {pendingEntry.amount}.");
#endif
                        for (int i = 0; i < pendingEntry.amount; ++i)
                        {
                            float destroyRespawnDelay = pendingEntry.destroyRespawnDelay;
                            if (destroyRespawnDelay <= 0f)
                                destroyRespawnDelay = this.destroyRespawnDelay;
                            Spawn(prefab, addressablePrefab, pendingEntry.level, 0, destroyRespawnDelay);
                        }
                    }
                    _pending.Clear();
                }
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
                int amount = GetRandomedSpawnAmount();
                for (int i = 0; i < amount; ++i)
                {
                    Spawn(prefab, addressablePrefab, Random.Range(minLevel, maxLevel + 1), 0, destroyRespawnDelay);
                }
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
                SpawnByAmount(prefab, addressablePrefab, spawningPrefab.level, spawningPrefab.amount, destroyRespawnDelay);
            }
        }

        public override void CancelAllSpawning()
        {
            _pending.Clear();
            foreach (CancellationTokenSource spawnCancellation in _spawnCancellations)
            {
                spawnCancellation.Cancel();
            }
            _spawnCancellations.Clear();
        }

        public virtual void SpawnByAmount(T prefab, AddressablePrefab addressablePrefab, int level, int amount, float destroyRespawnDelay)
        {
            for (int i = 0; i < amount; ++i)
            {
                Spawn(prefab, addressablePrefab, level, 0, destroyRespawnDelay);
            }
        }

        public virtual async void Spawn(T prefab, AddressablePrefab addressablePrefab, int level, float delay, float destroyRespawnDelay)
        {
            if (prefab == null && !addressablePrefab.IsDataValid())
                return;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            _spawnCancellations.Add(cancellationTokenSource);
            try
            {
                await SpawnRoutine(prefab, addressablePrefab, level, delay, destroyRespawnDelay, cancellationTokenSource);
            }
            catch { }
            finally
            {
                _spawnCancellations.Remove(cancellationTokenSource);
            }
        }

        async UniTask SpawnRoutine(T prefab, AddressablePrefab addressablePrefab, int level, float delay, float destroyRespawnDelay, CancellationTokenSource cancellationTokenSource)
        {
            await UniTask.Delay(Mathf.RoundToInt(delay * 1000), cancellationToken: cancellationTokenSource.Token);
            if (!AbleToSpawn())
            {
                Debug.Log($"Not able to spawn, Spawn Type={spawnType}, Spawn State={_subscribeHandler.CurrentSpawnState}");
                return;
            }
            T newEntity = SpawnInternal(prefab, addressablePrefab, level, destroyRespawnDelay);
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
                    destroyRespawnDelay = destroyRespawnDelay,
                });
            }
            else
            {
                // Store to entities collection, so the spawner can manage them later
                _subscribeHandler.AddEntity(newEntity);
            }
        }

        protected abstract T SpawnInternal(T prefab, AddressablePrefab addressablePrefab, int level, float destroyRespawnDelay);

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
                count += area.minAmount;
                List<SpawnPrefabData> spawningPrefabs = new List<SpawnPrefabData>(area.spawningPrefabs);
                foreach (SpawnPrefabData spawningPrefab in spawningPrefabs)
                {
                    count += spawningPrefab.amount;
                }
            }
            Debug.Log($"Spawning {typeof(T).Name} Amount: {count}");
        }

#if UNITY_EDITOR
        [ContextMenu("Fix invalid `respawnPendingEntitiesDelay` settings")]
        public virtual void FixInvalidRespawnPendingEntitiesDelaySettings()
        {
            GameSpawnArea[] spawnAreas = FindObjectsOfType<GameSpawnArea>(true);
            for (int i = 0; i < spawnAreas.Length; ++i)
            {
                if (spawnAreas[i].destroyRespawnDelay > 0f)
                    continue;
                spawnAreas[i].destroyRespawnDelay = spawnAreas[i].respawnPendingEntitiesDelay;
                spawnAreas[i].respawnPendingEntitiesDelay = 5f;
                Debug.Log($"[FIXED] {spawnAreas[i].name}, `destroyRespawnDelay` -> {spawnAreas[i].destroyRespawnDelay}", spawnAreas[i].gameObject);
                EditorUtility.SetDirty(spawnAreas[i]);
            }
        }
#endif
    }
}
