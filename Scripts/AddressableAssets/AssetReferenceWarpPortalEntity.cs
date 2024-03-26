using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceWarpPortalEntity : AssetReferenceLiteNetLibBehaviour<WarpPortalEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceWarpPortalEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}