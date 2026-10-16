using Cysharp.Threading.Tasks;
using Insthync.AddressableAssetTools;
using Insthync.UnityEditorUtils;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ImpactEffect
    {
        public UnityTag tag;
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS || DISABLE_ADDRESSABLES
#if !DISABLE_ADDRESSABLES
        [AddressableAssetConversion(nameof(addressableEffect))]
#endif
        public GameEffect effect;
#endif
#if !DISABLE_ADDRESSABLES
        public AssetReferenceGameEffect addressableEffect;
#endif

        public async void Play(Vector3 position, Quaternion rotation)
        {
#if !UNITY_SERVER
            GameEffect tempPrefab = null;
#if !EXCLUDE_PREFAB_REFS || DISABLE_ADDRESSABLES
            tempPrefab = effect;
#endif
#if !DISABLE_ADDRESSABLES
            AssetReferenceGameEffect tempAddressablePrefab = addressableEffect;
#endif
#if !DISABLE_ADDRESSABLES
            await ClientEffectPrefabs.SpawnAsync<GameEffect>(tempAddressablePrefab,
                prefab => PoolSystem.GetInstance(prefab, position, rotation), fallback: tempPrefab);
#else
            PoolSystem.GetInstance(tempPrefab, position, rotation);
#endif
#endif
            await UniTask.Yield();
        }
    }
}
