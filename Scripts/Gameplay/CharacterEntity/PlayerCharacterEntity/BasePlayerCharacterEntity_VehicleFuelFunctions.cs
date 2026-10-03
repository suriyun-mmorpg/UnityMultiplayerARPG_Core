using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        private float _lastVehicleRefuelTime = float.NegativeInfinity;

        public void CallCmdRefuelVehicle(uint vehicleObjectId, int itemIndex)
        {
            RPC(CmdRefuelVehicle, vehicleObjectId, itemIndex);
        }

        [ServerRpc]
        protected void CmdRefuelVehicle(uint vehicleObjectId, int itemIndex)
        {
#if UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES
            if (!IsServer || Time.unscaledTime - _lastVehicleRefuelTime < 0.25f)
                return;
            _lastVehicleRefuelTime = Time.unscaledTime;
            if (Manager.Assets.TryGetSpawnedObject(vehicleObjectId, out LiteNetLibIdentity identity) &&
                identity.TryGetComponent(out VehicleFuelComponent fuel))
                fuel.TryRefuelFromItem(this, itemIndex);
#endif
        }
    }
}
