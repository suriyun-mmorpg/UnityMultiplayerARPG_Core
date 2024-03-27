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

#if UNITY_EDITOR
        public AssetReferenceExpDropEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif

        public new AsyncOperationHandle<ExpDropEntity> InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return Addressables.ResourceManager.CreateChainOperation(Addressables.InstantiateAsync(RuntimeKey, position, rotation, parent, false), GameObjectReady);
        }

        public new AsyncOperationHandle<ExpDropEntity> InstantiateAsync(Transform parent = null, bool instantiateInWorldSpace = false)
        {
            return Addressables.ResourceManager.CreateChainOperation(Addressables.InstantiateAsync(RuntimeKey, parent, instantiateInWorldSpace, false), GameObjectReady);
        }

        public new AsyncOperationHandle<ExpDropEntity> LoadAssetAsync()
        {
            return Addressables.ResourceManager.CreateChainOperation(base.LoadAssetAsync<GameObject>(), GameObjectReady);
        }

        static AsyncOperationHandle<ExpDropEntity> GameObjectReady(AsyncOperationHandle<GameObject> arg)
        {
            var comp = arg.Result.GetComponent<ExpDropEntity>();
            return Addressables.ResourceManager.CreateCompletedOperation(comp, string.Empty);
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