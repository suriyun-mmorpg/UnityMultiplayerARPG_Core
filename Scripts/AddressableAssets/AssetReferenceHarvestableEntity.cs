using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceHarvestableEntity : AssetReferenceLiteNetLibBehaviour<HarvestableEntity>
    {
        public AssetReferenceHarvestableEntity(string guid) : base(guid)
        {
        }

#if UNITY_EDITOR
        public AssetReferenceHarvestableEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}