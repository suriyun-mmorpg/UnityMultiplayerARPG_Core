using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class AssetReferenceItemDropEntity : AssetReferenceLiteNetLibBehaviour<ItemDropEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceItemDropEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}