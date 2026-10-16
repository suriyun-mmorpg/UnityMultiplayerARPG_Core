#if !DISABLE_ADDRESSABLES
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MultiplayerARPG.Tests
{
    public class ClientEffectProbe : GameEffect
    {
        public int gets;
        public int returns;
        protected override void Awake() { }
        protected override void OnEnable() { }
        protected override void OnDisable() { }
        public override void InitPrefab() { }
        public override void OnGetInstance() { ++gets; }
        public override void OnPushBack() { ++returns; }
    }
    public class ClientEffectTestManager : LanRpgNetworkManager
    {
        protected override void OnDestroy() { }
    }
    public class EffectPrefabOperation : AsyncOperationBase<GameObject>
    {
        protected override void Execute() { }
        public void Finish(GameObject value) { Complete(value, true, (string)null); }
    }
    public class EffectPrefabTestLoader : EffectPrefabAssetLoader
    {
        public AsyncOperationHandle<GameObject> shared;
        public int loads;
        public int releases;
        protected override AsyncOperationHandle<GameObject> Load(object key)
        { ++loads; return Addressables.ResourceManager.Acquire(shared); }
        protected override void Release(AsyncOperationHandle<GameObject> handle)
        { ++releases; base.Release(handle); }
    }

    [Timeout(10000)]
    public class ClientEffectLoadingTests
    {
        private const string Prefix = "ClientEffectFixture";
        private const BindingFlags StaticPrivate = BindingFlags.Static | BindingFlags.NonPublic;
        private static readonly FieldInfo LoaderField = typeof(ClientEffectPrefabs).GetField("Loader", StaticPrivate);
        private Dictionary<IPoolDescriptor, Queue<IPoolDescriptor>> Pools =>
            (Dictionary<IPoolDescriptor, Queue<IPoolDescriptor>>)typeof(PoolSystem).GetField("pooledObjects", StaticPrivate).GetValue(null);
        private ClientEffectProbe prefab;
        private GameObject managerObject;
        private BaseGameNetworkManager previousManager;
        private object previousLoader;
        private EffectPrefabTestLoader loader;
        private EffectPrefabOperation operation;
        private readonly AssetReferenceGameEffect reference = new AssetReferenceGameEffect("0123456789abcdef0123456789abcdef");

        private static void SetManager(BaseGameNetworkManager value) =>
            typeof(BaseGameNetworkManager).GetProperty("Singleton").SetValue(null, value);

        private ClientEffectProbe[] Copies => Resources.FindObjectsOfTypeAll<ClientEffectProbe>().Where(p => p.name.StartsWith(Prefix)).ToArray();
        private void Trim() => typeof(PoolSystem).GetMethod("TrimClientEffectPools", StaticPrivate).Invoke(null, new object[] {Time.unscaledTime + 61f});
        private void ExpectDeferredDestroy() => LogAssert.Expect(LogType.Error, new Regex("Destroy.*edit mode", RegexOptions.IgnoreCase));
        private UniTask<ClientEffectProbe> Spawn(CancellationToken token = default) =>
            ClientEffectPrefabs.SpawnAsync<ClientEffectProbe>(reference, p => PoolSystem.GetInstance(p), token);

        [SetUp]
        public void SetUp()
        {
            previousManager = BaseGameNetworkManager.Singleton;
            SetManager(null);
            var root = new GameObject(Prefix);
            root.SetActive(false);
            prefab = root.AddComponent<ClientEffectProbe>();
            prefab.PoolSize = 30;
            operation = new EffectPrefabOperation();
            loader = new EffectPrefabTestLoader {shared = Addressables.ResourceManager.StartOperation(operation, default)};
            previousLoader = LoaderField.GetValue(null);
            LoaderField.SetValue(null, loader);
        }

        [TearDown]
        public void TearDown()
        {
            LoaderField.SetValue(null, previousLoader);
            SetManager(previousManager);
            if (managerObject != null) Object.DestroyImmediate(managerObject);
            foreach (var key in Pools.Keys.Where(p => (Object)p == null || p.gameObject.name.StartsWith(Prefix)).ToArray()) Pools.Remove(key);
            foreach (var copy in Copies) Object.DestroyImmediate(copy.gameObject);
            // Retire just the test states after removing their queues.
            Trim();
            if (loader.shared.IsValid()) Addressables.Release(loader.shared);
        }

        [Test]
        public void ClientSkipsAuthoredPrewarmingAndCreatesOnlyRequestedCopies()
        {
            PoolSystem.InitPool(prefab);
            Assert.IsEmpty(Pools[prefab]);
            Assert.AreEqual(1, Copies.Length);
            var first = PoolSystem.GetInstance(prefab);
            Assert.AreEqual(2, Copies.Length);
            PoolSystem.PushBack(first);
            Assert.AreSame(first, PoolSystem.GetInstance(prefab));
            Assert.AreEqual(2, first.gets);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(30)]
        public void ClientRetainsAtMostTwoCopiesWithoutIncreasingSmallerAuthoredLimits(int capacity)
        {
            prefab.PoolSize = capacity;
            var copies = Enumerable.Range(0, 4).Select(_ => PoolSystem.GetInstance(prefab)).ToArray();
            int retained = Math.Min(2, capacity);
            for (int i = 0; i < copies.Length; ++i)
            {
                if (i >= retained) ExpectDeferredDestroy();
                PoolSystem.PushBack(copies[i]);
                Assert.AreEqual(1, copies[i].returns, "Excess copies must still finish pending impact callbacks");
            }
            Assert.AreEqual(retained, Pools[prefab].Count);
        }

        [Test]
        public void ServerAndHostKeepAuthoredPrewarmAndRetention()
        {
            managerObject = new GameObject("Effect server fixture");
            managerObject.SetActive(false);
            var manager = managerObject.AddComponent<ClientEffectTestManager>();
            typeof(LiteNetLibManager.LiteNetLibManager).GetProperty("IsServer").SetValue(manager, true);
            SetManager(manager);
            prefab.PoolSize = 4;
            PoolSystem.InitPool(prefab);
            Assert.AreEqual(4, Pools[prefab].Count);
            var active = Enumerable.Range(0, 4).Select(_ => PoolSystem.GetInstance(prefab)).ToArray();
            foreach (var copy in active) PoolSystem.PushBack(copy);
            Assert.AreEqual(4, Pools[prefab].Count);
            Assert.IsTrue(active.All(p => p.GetComponent<ClientEffectPoolCopy>() == null));
        }

        [Test]
        public void IdleTrimLeavesActiveEffectsAliveAndTheirLateReturnCannotEnterANewPool()
        {
            var active = PoolSystem.GetInstance(prefab);
            Trim();
            Assert.IsTrue(active.gameObject.activeSelf);
            var replacement = PoolSystem.GetInstance(prefab);
            ExpectDeferredDestroy();
            PoolSystem.PushBack(active);
            Assert.AreEqual(1, active.returns);
            Assert.IsEmpty(Pools[prefab]);
            PoolSystem.PushBack(replacement);
            Assert.AreEqual(1, Pools[prefab].Count);
        }

        [UnityTest]
        public IEnumerator ReusedCopyKeepsOneAssetLeaseUntilDestruction() => UniTask.ToCoroutine(async () =>
        {
            var pending = Spawn();
            operation.Finish(prefab.gameObject);
            var first = await pending;
            PoolSystem.PushBack(first);
            var reused = await Spawn();
            Assert.AreSame(first, reused);
            await UniTask.DelayFrame(2);
            Assert.AreEqual(1, loader.releases, "Only the redundant request lease is released");
            Object.DestroyImmediate(first.gameObject);
            await UniTask.DelayFrame(2);
            Assert.AreEqual(2, loader.releases);
        });

        [Test]
        public void ClearRetiresOldActiveCopiesWithoutEmittingDisconnectImpacts()
        {
            var active = PoolSystem.GetInstance(prefab);
            PoolSystem.Clear();
            ExpectDeferredDestroy();
            PoolSystem.PushBack(active);
            Assert.AreEqual(0, active.returns);
        }

        [UnityTest]
        public IEnumerator WrongPrefabComponentIsReportedAndItsLeaseReleased() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.Expect(LogType.Error, new Regex("\\[Client Effects\\] Unable to spawn.*has no ProjectileEffect", RegexOptions.Singleline));
            var pending = ClientEffectPrefabs.SpawnAsync<ProjectileEffect>(reference, p => PoolSystem.GetInstance(p));
            operation.Finish(prefab.gameObject);
            Assert.IsNull(await pending);
            await UniTask.DelayFrame(2);
            Assert.AreEqual(1, loader.releases);
        });

        [UnityTest]
        public IEnumerator CancelingOnePendingSpawnDoesNotBreakAnotherSharedLoad() => UniTask.ToCoroutine(async () =>
        {
            using var token = new CancellationTokenSource();
            var canceled = Spawn(token.Token);
            var live = Spawn();
            token.Cancel();
            Assert.IsNull(await canceled);
            operation.Finish(prefab.gameObject);
            var instance = await live;
            Assert.IsNotNull(instance);
            Assert.AreEqual(2, Copies.Length);
            Assert.AreEqual(1, loader.releases);
            Object.DestroyImmediate(instance.gameObject);
            await UniTask.DelayFrame(2);
            Assert.AreEqual(2, loader.releases);
        });

        [UnityTest]
        public IEnumerator ClearDiscardsLateSpawnAndReleasesItsAssetReference() => UniTask.ToCoroutine(async () =>
        {
            var pending = Spawn();
            PoolSystem.Clear();
            operation.Finish(prefab.gameObject);
            Assert.IsNull(await pending);
            Assert.AreEqual(1, Copies.Length);
            await UniTask.DelayFrame(2);
            Assert.AreEqual(1, loader.releases);
        });

        [UnityTest]
        public IEnumerator IdlePoolReleasesPrefabAfterPooledCopiesAreDestroyed() => UniTask.ToCoroutine(async () =>
        {
            var pending = Spawn();
            operation.Finish(prefab.gameObject);
            var first = await pending;
            PoolSystem.PushBack(first);
            ExpectDeferredDestroy();
            Trim();
            Assert.IsFalse(Pools.ContainsKey(prefab));
            Assert.AreEqual(0, loader.releases);
            Object.DestroyImmediate(first.gameObject);
            await UniTask.DelayFrame(2);
            Assert.AreEqual(1, loader.releases);
        });

        [UnityTest]
        public IEnumerator DirectPrefabFallbackDoesNotLoadAnAddressable() => UniTask.ToCoroutine(async () =>
        {
            var instance = await ClientEffectPrefabs.SpawnAsync<ClientEffectProbe>(null,
                p => PoolSystem.GetInstance(p), fallback: prefab);
            Assert.IsNotNull(instance);
            Assert.AreEqual(0, loader.loads);
            Assert.AreEqual(2, Copies.Length);
        });

        [UnityTest]
        public IEnumerator CanceledRequestDoesNotSpawnDirectFallback() => UniTask.ToCoroutine(async () =>
        {
            using var token = new CancellationTokenSource();
            token.Cancel();
            var instance = await ClientEffectPrefabs.SpawnAsync<ClientEffectProbe>(null,
                p => PoolSystem.GetInstance(p), token.Token, prefab);
            Assert.IsNull(instance);
            Assert.AreEqual(0, loader.loads);
            Assert.AreEqual(1, Copies.Length);
        });

        [UnityTest]
        public IEnumerator MissingComponentUsesFallbackAndReleasesAddressable() => UniTask.ToCoroutine(async () =>
        {
            var wrong = new GameObject(Prefix + "Wrong");
            try
            {
                LogAssert.Expect(LogType.Error, new Regex("\\[Client Effects\\] Unable to spawn.*has no ClientEffectProbe", RegexOptions.Singleline));
                var pending = ClientEffectPrefabs.SpawnAsync<ClientEffectProbe>(reference,
                    p => PoolSystem.GetInstance(p), fallback: prefab);
                operation.Finish(wrong);
                Assert.IsNotNull(await pending);
                await UniTask.DelayFrame(2);
                Assert.AreEqual(1, loader.releases);
                Assert.AreEqual(2, Copies.Length);
            }
            finally { Object.DestroyImmediate(wrong); }
        });
    }
}
#endif
