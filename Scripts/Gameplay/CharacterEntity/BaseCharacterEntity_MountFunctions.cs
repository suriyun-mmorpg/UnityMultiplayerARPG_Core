using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public void Mount(MountEntity mountEntityPrefab)
        {
            if (!IsServer || mountEntityPrefab == null || Time.unscaledTime - lastMountTime < MOUNT_DELAY)
                return;

            lastMountTime = Time.unscaledTime;

            Vector3 enterPosition = CacheTransform.position;
            if (PassengingVehicle.objectId > 0)
                enterPosition = ExitVehicle();

            // Instantiate new mount entity
            GameObject spawnObj = Instantiate(mountEntityPrefab.gameObject, enterPosition, Quaternion.Euler(0, CacheTransform.eulerAngles.y, 0));
            MountEntity vehicle = BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj, 0, ConnectionId).GetComponent<MountEntity>();

            // Seat index for mount entity always 0
            EnterVehicle(vehicle, 0);
        }
    }
}
