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
                ExitVehicle();
            EnterVehicle(mountEntityPrefab, 0);
        }
    }
}
