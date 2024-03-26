using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceBuildingEntity : AssetReferenceLiteNetLibBehaviour<BuildingEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceBuildingEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}