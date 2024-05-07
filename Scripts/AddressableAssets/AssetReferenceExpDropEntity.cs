using LiteNetLibManager;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceExpDropEntity : AssetReferenceBaseRewardDropEntity
    {
        public AssetReferenceExpDropEntity(string guid) : base(guid)
        {
        }

        public new AsyncOperationHandle<ExpDropEntity> InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return Addressables.ResourceManager.CreateChainOperation(Addressables.InstantiateAsync(RuntimeKey, position, rotation, parent, false), GetComponentChainOperation);
        }

        public new AsyncOperationHandle<ExpDropEntity> InstantiateAsync(Transform parent = null, bool instantiateInWorldSpace = false)
        {
            return Addressables.ResourceManager.CreateChainOperation(Addressables.InstantiateAsync(RuntimeKey, parent, instantiateInWorldSpace, false), GetComponentChainOperation);
        }

        public new AsyncOperationHandle<ExpDropEntity> LoadAssetAsync()
        {
            return Addressables.ResourceManager.CreateChainOperation(base.LoadAssetAsync<GameObject>(), GetComponentChainOperation);
        }

        private static AsyncOperationHandle<ExpDropEntity> GetComponentChainOperation(AsyncOperationHandle<GameObject> handler)
        {
            return Addressables.ResourceManager.CreateCompletedOperation(handler.Result.GetComponent<ExpDropEntity>(), string.Empty);
        }

        public override bool ValidateAsset(Object obj)
        {
            return ValidateAsset<ExpDropEntity>(obj);
        }

        public override bool ValidateAsset(string path)
        {
            return ValidateAsset<ExpDropEntity>(path);
        }
    }
}