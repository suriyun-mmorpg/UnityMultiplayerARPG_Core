using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceItemsContainerEntity : AssetReferenceLiteNetLibBehaviour<ItemsContainerEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceItemsContainerEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}