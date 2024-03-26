using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class AssetReferenceWarpPortalEntity : AssetReferenceLiteNetLibBehaviour<WarpPortalEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceWarpPortalEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}