using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class AssetReferenceBuildingEntity : AssetReferenceLiteNetLibBehaviour<BuildingEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceBuildingEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}