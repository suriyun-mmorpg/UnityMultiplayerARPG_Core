using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG
{
    public static partial class PoolSystem
    {
        private const int ClientEffectRetention = 2;
        private const float ClientEffectIdleSeconds = 60f;
        private static readonly Dictionary<IPoolDescriptor, ClientEffectPoolState> clientEffectPools = new Dictionary<IPoolDescriptor, ClientEffectPoolState>();
        private static readonly List<IPoolDescriptor> expiredClientEffectPools = new List<IPoolDescriptor>();
        private static ClientEffectPoolMaintenance effectMaintenance;
        internal static int ClientEffectGeneration { get; private set; }

        internal static bool UseClientEffectPolicy
        {
            get
            {
#if UNITY_SERVER
                return false;
#else
                return !Application.isBatchMode && (BaseGameNetworkManager.Singleton == null || !BaseGameNetworkManager.Singleton.IsServer);
#endif
            }
        }

        private static bool IsClientEffect(IPoolDescriptor prefab) => UseClientEffectPolicy && (prefab is GameEffect || prefab is ProjectileEffect);
        private static int GetEffectRetentionCapacity(IPoolDescriptor prefab) => IsClientEffect(prefab)
            ? Mathf.Min(ClientEffectRetention, Mathf.Max(0, prefab.PoolSize)) : prefab.PoolSize;

        private static void PrepareClientEffectPool(IPoolDescriptor prefab)
        {
            if (!IsClientEffect(prefab)) return;
            clientEffectPools[prefab] = new ClientEffectPoolState { LastUse = Time.unscaledTime, Generation = ClientEffectGeneration };
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[Client Effects] On-demand pool {prefab.gameObject.name}: no prewarm, retain up to {GetEffectRetentionCapacity(prefab)} idle copies.");
#endif
            if (Application.isPlaying && effectMaintenance == null)
            {
                effectMaintenance = new GameObject("_ClientEffectPools").AddComponent<ClientEffectPoolMaintenance>();
                UnityEngine.Object.DontDestroyOnLoad(effectMaintenance.gameObject);
            }
        }

        private static void RegisterClientEffectCopy(IPoolDescriptor prefab, IPoolDescriptor instance)
        {
            if (clientEffectPools.TryGetValue(prefab, out var state))
                instance.gameObject.AddComponent<ClientEffectPoolCopy>().Pool = state;
        }

        private static bool TryReturnRetiredClientEffect(IPoolDescriptor instance)
        {
            if (!instance.gameObject.TryGetComponent<ClientEffectPoolCopy>(out var copy) || copy.Pool == null || !copy.Pool.Retired)
                return false;
            // Flush completion work such as pending projectile impacts after idle eviction,
            // but never emit effects from a session which has already disconnected.
            if (copy.Pool.Generation == ClientEffectGeneration) instance.OnPushBack();
            UnityEngine.Object.Destroy(instance.gameObject);
            return true;
        }

        private static void TouchClientEffectPool(IPoolDescriptor prefab)
        {
            if (clientEffectPools.TryGetValue(prefab, out var state)) state.LastUse = Time.unscaledTime;
        }

        private static void RetireClientEffectPools()
        {
            ++ClientEffectGeneration;
            foreach (var state in clientEffectPools.Values) state.Retired = true;
            clientEffectPools.Clear();
        }

        internal static void TrimClientEffectPools(float now)
        {
            expiredClientEffectPools.Clear();
            foreach (var pair in clientEffectPools)
                if (now - pair.Value.LastUse >= ClientEffectIdleSeconds) expiredClientEffectPools.Add(pair.Key);
            foreach (var prefab in expiredClientEffectPools)
            {
                clientEffectPools[prefab].Retired = true;
                clientEffectPools.Remove(prefab);
                if (!pooledObjects.TryGetValue(prefab, out var queue)) continue;
                int removed = 0;
                while (queue.Count > 0)
                {
                    var copy = queue.Dequeue();
                    if ((UnityEngine.Object)copy == null) continue;
                    UnityEngine.Object.Destroy(copy.gameObject);
                    ++removed;
                }
                pooledObjects.Remove(prefab);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (removed > 0) Debug.Log($"[Client Effects] Trimmed {removed} idle copies of {prefab.gameObject.name}.");
#endif
            }
            expiredClientEffectPools.Clear();
        }
    }

    internal sealed class ClientEffectPoolState
    {
        internal float LastUse;
        internal bool Retired;
        internal int Generation;
    }

    // Retiring a pool never interrupts its active effects. Their asset lease stays alive
    // until the effect finishes or its scene is destroyed, then return destroys the copy.
    public sealed class ClientEffectPoolCopy : MonoBehaviour
    {
        internal ClientEffectPoolState Pool;
        private IDisposable assetLease;
        private CancellationTokenRegistration destructionRegistration;
        internal void HoldAsset(IDisposable lease)
        {
            if (assetLease != null) { lease.Dispose(); return; }
            assetLease = lease;
            // Also handles a copy destroyed before Unity has invoked its Awake.
            destructionRegistration = gameObject.GetCancellationTokenOnDestroy()
                .RegisterWithoutCaptureExecutionContext(owner => ((ClientEffectPoolCopy)owner).ReleaseAsset(), this);
        }
        private void OnDestroy()
        {
            ReleaseAsset();
            destructionRegistration.Dispose();
        }
        private void ReleaseAsset()
        {
            assetLease?.Dispose();
            assetLease = null;
        }
    }

    public sealed class ClientEffectPoolMaintenance : MonoBehaviour
    {
        private float nextCheck;
        private void Update()
        {
            if (Time.unscaledTime < nextCheck) return;
            nextCheck = Time.unscaledTime + 5f;
            PoolSystem.TrimClientEffectPools(Time.unscaledTime);
        }
    }
}
