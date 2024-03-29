using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceItemsContainerEntity : AssetReferenceLiteNetLibBehaviour<ItemsContainerEntity>
    {
        public AssetReferenceItemsContainerEntity(string guid) : base(guid)
        {
        }

#if UNITY_EDITOR
        public AssetReferenceItemsContainerEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}