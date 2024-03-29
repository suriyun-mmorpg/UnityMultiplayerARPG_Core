using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceItemDropEntity : AssetReferenceLiteNetLibBehaviour<ItemDropEntity>
    {
        public AssetReferenceItemDropEntity(string guid) : base(guid)
        {
        }

#if UNITY_EDITOR
        public AssetReferenceItemDropEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}