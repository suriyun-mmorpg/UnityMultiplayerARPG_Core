using LiteNetLibManager;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceCurrencyDropEntity : AssetReferenceBaseRewardDropEntity
    {
        public AssetReferenceCurrencyDropEntity(string guid) : base(guid)
        {
        }

#if UNITY_EDITOR
        public AssetReferenceCurrencyDropEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif

        public new AsyncOperationHandle<CurrencyDropEntity> InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return Addressables.ResourceManager.CreateChainOperation(Addressables.InstantiateAsync(RuntimeKey, position, rotation, parent, false), GameObjectReady);
        }

        public new AsyncOperationHandle<CurrencyDropEntity> InstantiateAsync(Transform parent = null, bool instantiateInWorldSpace = false)
        {
            return Addressables.ResourceManager.CreateChainOperation(Addressables.InstantiateAsync(RuntimeKey, parent, instantiateInWorldSpace, false), GameObjectReady);
        }

        public new AsyncOperationHandle<CurrencyDropEntity> LoadAssetAsync()
        {
            return Addressables.ResourceManager.CreateChainOperation(base.LoadAssetAsync<GameObject>(), GameObjectReady);
        }

        static AsyncOperationHandle<CurrencyDropEntity> GameObjectReady(AsyncOperationHandle<GameObject> arg)
        {
            var comp = arg.Result.GetComponent<CurrencyDropEntity>();
            return Addressables.ResourceManager.CreateCompletedOperation(comp, string.Empty);
        }

        public override bool ValidateAsset(Object obj)
        {
            return ValidateAsset<CurrencyDropEntity>(obj);
        }

        public override bool ValidateAsset(string path)
        {
            return ValidateAsset<CurrencyDropEntity>(path);
        }
    }
}