using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceHarvestableEntity : AssetReferenceLiteNetLibBehaviour<HarvestableEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceHarvestableEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}