using Insthync.AddressableAssetTools;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class GameEffectPoolContainer : IAddressableAssetConversable
    {
#if !UNITY_SERVER || UNITY_EDITOR
        public Transform container;
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        public GameEffect prefab;
#endif
        public AssetReferenceGameEffect addressablePrefab;
#endif

        public async void GetInstance()
        {
#if !UNITY_SERVER
            GameEffect tempPrefab = null;
#if !EXCLUDE_PREFAB_REFS
            tempPrefab = prefab;
#endif
            AssetReferenceGameEffect tempAddressablePrefab = addressablePrefab;
            GameEffect loadedPrefab = await tempAddressablePrefab.GetOrLoadAssetAsyncOrUsePrefab(tempPrefab);
            if (loadedPrefab != null)
                PoolSystem.GetInstance(loadedPrefab, container.position, container.rotation).FollowingTarget = container;
#endif
        }

        public void ProceedAddressableAssetConversion(string groupName)
        {
#if UNITY_EDITOR
            AddressableEditorUtils.ConvertObjectRefToAddressable(ref prefab, ref addressablePrefab, groupName);
#endif
        }
    }
}