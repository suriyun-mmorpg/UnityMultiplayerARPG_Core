using LiteNetLibManager;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceBaseMonsterCharacterEntity : AssetReferenceBaseCharacterEntity
    {
        public AssetReferenceBaseMonsterCharacterEntity(string guid) : base(guid)
        {
        }

#if UNITY_EDITOR
        public AssetReferenceBaseMonsterCharacterEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif

        public new AsyncOperationHandle<BaseMonsterCharacterEntity> InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return Addressables.ResourceManager.CreateChainOperation(Addressables.InstantiateAsync(RuntimeKey, position, rotation, parent, false), GetComponentChainOperation);
        }

        public new AsyncOperationHandle<BaseMonsterCharacterEntity> InstantiateAsync(Transform parent = null, bool instantiateInWorldSpace = false)
        {
            return Addressables.ResourceManager.CreateChainOperation(Addressables.InstantiateAsync(RuntimeKey, parent, instantiateInWorldSpace, false), GetComponentChainOperation);
        }

        public new AsyncOperationHandle<BaseMonsterCharacterEntity> LoadAssetAsync()
        {
            return Addressables.ResourceManager.CreateChainOperation(base.LoadAssetAsync<GameObject>(), GetComponentChainOperation);
        }

        private static AsyncOperationHandle<BaseMonsterCharacterEntity> GetComponentChainOperation(AsyncOperationHandle<GameObject> handler)
        {
            return Addressables.ResourceManager.CreateCompletedOperation(handler.Result.GetComponent<BaseMonsterCharacterEntity>(), string.Empty);
        }

        public override bool ValidateAsset(Object obj)
        {
            return ValidateAsset<BaseMonsterCharacterEntity>(obj);
        }

        public override bool ValidateAsset(string path)
        {
            return ValidateAsset<BaseMonsterCharacterEntity>(path);
        }
    }
}