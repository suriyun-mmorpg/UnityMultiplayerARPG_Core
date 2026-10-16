#if !DISABLE_ADDRESSABLES
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Insthync.AddressableAssetTools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MultiplayerARPG
{
    public class EffectPrefabAssetLoader
    {
        public sealed class Lease : IDisposable
        {
            private EffectPrefabAssetLoader owner;
            private readonly AsyncOperationHandle<GameObject> handle;
            public GameObject Asset { get; }
            internal Lease(EffectPrefabAssetLoader owner, AsyncOperationHandle<GameObject> handle, GameObject asset)
            { this.owner = owner; this.handle = handle; Asset = asset; }
            public void Dispose()
            {
                if (owner == null) return;
                var releasing = owner;
                owner = null;
                releasing.ReleaseAfterDestruction(handle).Forget();
            }
        }

        protected virtual AsyncOperationHandle<GameObject> Load(object key) => Addressables.LoadAssetAsync<GameObject>(key);
        protected virtual void Release(AsyncOperationHandle<GameObject> handle) => Addressables.Release(handle);
        public async UniTask<Lease> AcquireAsync(AssetReference reference, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var handle = Load(reference.RuntimeKey);
            try
            {
                var asset = await handle.ToUniTask(cancellationToken: token, cancelImmediately: true);
                token.ThrowIfCancellationRequested();
                if (asset == null) throw new InvalidOperationException($"Effect prefab {reference.RuntimeKey} is missing.");
                return new Lease(this, handle, asset);
            }
            catch
            {
                if (handle.IsValid()) Release(handle);
                throw;
            }
        }
        private async UniTask ReleaseAfterDestruction(AsyncOperationHandle<GameObject> handle)
        {
            await UniTask.DelayFrame(1, PlayerLoopTiming.LastPostLateUpdate);
            if (handle.IsValid()) Release(handle);
        }
    }

    public static class ClientEffectPrefabs
    {
        internal static EffectPrefabAssetLoader Loader = new EffectPrefabAssetLoader();

        public static async UniTask<T> SpawnAsync<T>(AssetReference reference, Func<T, T> spawn, CancellationToken token = default, T fallback = null)
            where T : Component, IPoolDescriptor
        {
            if (token.IsCancellationRequested) return null;
            if (reference == null || !reference.RuntimeKeyIsValid())
                return fallback != null ? spawn(fallback) : null;
            if (!PoolSystem.UseClientEffectPolicy)
            {
                // Hosts and servers retain the original global asset cache and pool policy.
                var legacy = await reference.GetOrLoadAssetAsyncOrUsePrefab(fallback);
                return legacy != null && !token.IsCancellationRequested ? spawn(legacy) : null;
            }
            int generation = PoolSystem.ClientEffectGeneration;
            EffectPrefabAssetLoader.Lease lease = null;
            try
            {
                lease = await Loader.AcquireAsync(reference, token);
                if (token.IsCancellationRequested || generation != PoolSystem.ClientEffectGeneration) return null;
                var prefab = lease.Asset.GetComponent<T>();
                if (prefab == null) throw new InvalidOperationException($"Effect prefab {lease.Asset.name} has no {typeof(T).Name}.");
                var instance = spawn(prefab);
                if (instance == null) return null;
                if (!instance.TryGetComponent<ClientEffectPoolCopy>(out var copy)) copy = instance.gameObject.AddComponent<ClientEffectPoolCopy>();
                copy.HoldAsset(lease);
                lease = null;
                return instance;
            }
            catch (OperationCanceledException) { return null; }
            catch (Exception error)
            {
                Debug.LogError($"[Client Effects] Unable to spawn {reference.RuntimeKey}: {error}");
                return fallback != null && !token.IsCancellationRequested && generation == PoolSystem.ClientEffectGeneration
                    ? spawn(fallback) : null;
            }
            finally { lease?.Dispose(); }
        }
    }
}

#endif
