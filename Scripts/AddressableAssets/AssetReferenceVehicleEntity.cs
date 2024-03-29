using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceVehicleEntity : AssetReferenceLiteNetLibBehaviour<VehicleEntity>
    {
        public AssetReferenceVehicleEntity(string guid) : base(guid)
        {
        }

#if UNITY_EDITOR
        public AssetReferenceVehicleEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}