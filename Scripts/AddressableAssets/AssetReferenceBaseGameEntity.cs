using LiteNetLibManager;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceBaseGameEntity<T> : AssetReferenceLiteNetLibBehaviour<BaseGameEntity>
        where T : BaseGameEntity
    {
        public AssetReferenceBaseGameEntity(string guid) : base(guid)
        {
        }

#if UNITY_EDITOR
        public AssetReferenceBaseGameEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif

        public new AsyncOperationHandle<T> InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return Addressables.ResourceManager.CreateChainOperation(Addressables.InstantiateAsync(RuntimeKey, position, rotation, parent, false), GetComponentChainOperation);
        }

        public new AsyncOperationHandle<T> InstantiateAsync(Transform parent = null, bool instantiateInWorldSpace = false)
        {
            return Addressables.ResourceManager.CreateChainOperation(Addressables.InstantiateAsync(RuntimeKey, parent, instantiateInWorldSpace, false), GetComponentChainOperation);
        }

        public new AsyncOperationHandle<T> LoadAssetAsync()
        {
            return Addressables.ResourceManager.CreateChainOperation(base.LoadAssetAsync<GameObject>(), GetComponentChainOperation);
        }

        private static AsyncOperationHandle<T> GetComponentChainOperation(AsyncOperationHandle<GameObject> handler)
        {
            return Addressables.ResourceManager.CreateCompletedOperation(handler.Result.GetComponent<T>(), string.Empty);
        }

        public override bool ValidateAsset(Object obj)
        {
            return ValidateAsset<T>(obj);
        }

        public override bool ValidateAsset(string path)
        {
            return ValidateAsset<T>(path);
        }
    }
}