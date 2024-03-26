using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class AssetReferenceHarvestableEntity : AssetReferenceLiteNetLibBehaviour<HarvestableEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceHarvestableEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}