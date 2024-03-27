using LiteNetLibManager;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceBasePlayerCharacterEntity : AssetReferenceBaseCharacterEntity
    {
        public AssetReferenceBasePlayerCharacterEntity(string guid) : base(guid)
        {
        }

#if UNITY_EDITOR
        public AssetReferenceBasePlayerCharacterEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif

        public new AsyncOperationHandle<BasePlayerCharacterEntity> InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return Addressables.ResourceManager.CreateChainOperation(Addressables.InstantiateAsync(RuntimeKey, position, rotation, parent, false), GameObjectReady);
        }

        public new AsyncOperationHandle<BasePlayerCharacterEntity> InstantiateAsync(Transform parent = null, bool instantiateInWorldSpace = false)
        {
            return Addressables.ResourceManager.CreateChainOperation(Addressables.InstantiateAsync(RuntimeKey, parent, instantiateInWorldSpace, false), GameObjectReady);
        }

        public new AsyncOperationHandle<BasePlayerCharacterEntity> LoadAssetAsync()
        {
            return Addressables.ResourceManager.CreateChainOperation(base.LoadAssetAsync<GameObject>(), GameObjectReady);
        }

        static AsyncOperationHandle<BasePlayerCharacterEntity> GameObjectReady(AsyncOperationHandle<GameObject> arg)
        {
            var comp = arg.Result.GetComponent<BasePlayerCharacterEntity>();
            return Addressables.ResourceManager.CreateCompletedOperation(comp, string.Empty);
        }

        public override bool ValidateAsset(Object obj)
        {
            return ValidateAsset<BasePlayerCharacterEntity>(obj);
        }

        public override bool ValidateAsset(string path)
        {
            return ValidateAsset<BasePlayerCharacterEntity>(path);
        }
    }
}