using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public void Mount(VehicleEntity mountEntityPrefab)
        {
            if (!IsServer || mountEntityPrefab == null || Time.unscaledTime - lastMountTime < MOUNT_DELAY)
                return;

            lastMountTime = Time.unscaledTime;

            Vector3 enterPosition = CacheTransform.position;
            if (PassengingVehicleEntity != null)
            {
                enterPosition = PassengingVehicleEntity.Entity.CacheTransform.position;
                ExitVehicle();
            }

            // Instantiate new mount entity
            LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                mountEntityPrefab.Identity.HashAssetId, enterPosition,
                Quaternion.Euler(0, CacheTransform.eulerAngles.y, 0));
            VehicleEntity vehicle = spawnObj.GetComponent<VehicleEntity>();
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);

            // Seat index for mount entity always 0
            EnterVehicle(vehicle, 0);
        }
    }
}
