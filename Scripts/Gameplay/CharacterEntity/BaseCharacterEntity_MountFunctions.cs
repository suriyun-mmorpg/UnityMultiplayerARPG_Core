using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        protected virtual void ApplyItemMount(Item item, short level)
        {
            if (IsDead() || !IsServer || item == null || level <= 0)
                return;

            Mount(item.mountEntity);
        }

        protected virtual void ApplySkillMount(Skill skill, short level)
        {
            if (IsDead() || !IsServer || skill == null || level <= 0)
                return;

            Mount(skill.mount.mountEntity);
        }

        protected void Mount(MountEntity mountEntityPrefab)
        {
            if (!IsServer || mountEntityPrefab == null)
                return;

            // Unmount
            UnMount();

            // Instantiate new mount entity
            GameObject spawnObj = Instantiate(mountEntityPrefab.gameObject, CacheTransform.position, CacheTransform.rotation);
            MountEntity mountEntity = BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj).GetComponent<MountEntity>();

            // Set mount info
            RidingVehicle ridingVehicle = new RidingVehicle()
            {
                vehicleObjectId = mountEntity.ObjectId,
                seatIndex = 0   // Seat index for mount entity always 0
            };
            RidingVehicle = ridingVehicle;
        }

        protected void UnMount()
        {
            if (!IsServer || RidingVehicle.vehicleObjectId == 0)
                return;

            // Destroy mount entity
            LiteNetLibIdentity identity;
            if (BaseGameNetworkManager.Singleton.Assets.TryGetSpawnedObject(RidingVehicle.vehicleObjectId, out identity))
                identity.NetworkDestroy();

            // Clear riding vehicle data
            RidingVehicle ridingVehicle = RidingVehicle;
            ridingVehicle.vehicleObjectId = 0;
            ridingVehicle.seatIndex = 0;
            RidingVehicle = ridingVehicle;
        }
    }
}
