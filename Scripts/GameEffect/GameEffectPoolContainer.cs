using Cysharp.Threading.Tasks;
using Insthync.AddressableAssetTools;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class GameEffectPoolContainer : IAddressableAssetConversable
    {
#if !UNITY_SERVER || UNITY_EDITOR
        public Transform container;
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS || DISABLE_ADDRESSABLES
        public GameEffect prefab;
#endif
#if !DISABLE_ADDRESSABLES
        public AssetReferenceGameEffect addressablePrefab;
#endif
#endif

        public async void GetInstance()
        {
#if !UNITY_SERVER
            Transform target = container;
            if (target == null) return;
            var token = target.gameObject.GetCancellationTokenOnDestroy();
            GameEffect tempPrefab = null;
#if !EXCLUDE_PREFAB_REFS || DISABLE_ADDRESSABLES
            tempPrefab = prefab;
#endif
#if !DISABLE_ADDRESSABLES
            AssetReferenceGameEffect tempAddressablePrefab = addressablePrefab;
#endif
            GameEffect instance;
#if !DISABLE_ADDRESSABLES
            instance = await ClientEffectPrefabs.SpawnAsync<GameEffect>(tempAddressablePrefab,
                prefab => PoolSystem.GetInstance(prefab, target.position, target.rotation), token, tempPrefab);
#else
            await UniTask.Yield();
            if (token.IsCancellationRequested) return;
            instance = PoolSystem.GetInstance(tempPrefab, target.position, target.rotation);
#endif
            if (instance != null && target != null)
                instance.FollowingTarget = target;
#endif
        }

        public void ProceedAddressableAssetConversion(string groupName)
        {
#if UNITY_EDITOR && !DISABLE_ADDRESSABLES
            AddressableEditorUtils.ConvertObjectRefToAddressable(ref prefab, ref addressablePrefab, groupName);
#endif
        }
    }
}