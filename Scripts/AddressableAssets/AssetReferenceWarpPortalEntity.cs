using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceWarpPortalEntity : AssetReferenceLiteNetLibBehaviour<WarpPortalEntity>
    {
        public AssetReferenceWarpPortalEntity(string guid) : base(guid)
        {
        }

#if UNITY_EDITOR
        public AssetReferenceWarpPortalEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}