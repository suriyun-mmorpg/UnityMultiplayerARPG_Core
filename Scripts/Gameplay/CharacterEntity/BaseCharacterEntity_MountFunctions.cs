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
            if (RidingVehicle.objectId > 0)
                UnMount();
            EnterVehicle(mountEntityPrefab, 0);
        }

        protected void UnMount()
        {
            if (!IsServer || RidingVehicle.objectId == 0)
                return;

            uint vehicleObjectId = RidingVehicle.objectId;

            // Exit vehicle before destroy mount entity
            ExitVehicle();

            // Destroy mount entity
            LiteNetLibIdentity identity;
            if (BaseGameNetworkManager.Singleton.Assets.TryGetSpawnedObject(vehicleObjectId, out identity))
                identity.NetworkDestroy();
        }
    }
}
