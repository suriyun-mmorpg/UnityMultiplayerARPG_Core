using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceBuildingEntity : AssetReferenceLiteNetLibBehaviour<BuildingEntity>
    {
        public AssetReferenceBuildingEntity(string guid) : base(guid)
        {
        }

#if UNITY_EDITOR
        public AssetReferenceBuildingEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}