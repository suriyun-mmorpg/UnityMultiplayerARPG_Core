using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class AssetReferenceVehicleEntity : AssetReferenceLiteNetLibBehaviour<VehicleEntity>
    {
#if UNITY_EDITOR

        public AssetReferenceVehicleEntity(LiteNetLibBehaviour behaviour) : base(behaviour)
        {
        }
#endif
    }
}