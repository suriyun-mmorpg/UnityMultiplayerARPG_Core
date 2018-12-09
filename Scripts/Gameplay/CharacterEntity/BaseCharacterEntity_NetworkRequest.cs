using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public virtual void RequestAttack()
        {
            if (!CanMoveOrDoActions())
                return;
            CallNetFunction(NetFuncAttack, FunctionReceivers.Server);
        }

        public virtual void RequestUseSkill(Vector3 position, int dataId)
        {
            if (!CanMoveOrDoActions())
                return;
            CallNetFunction(NetFuncUseSkill, FunctionReceivers.Server, position, dataId);
        }

        public virtual void RequestUseItem(ushort index)
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncUseItem, FunctionReceivers.Server, index);
        }

        public virtual void RequestPlayActionAnimation(AnimActionType animActionType, int dataId, byte index)
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncPlayActionAnimation, FunctionReceivers.All, (byte)animActionType, dataId, index);
        }

        public virtual void RequestPickupItem(uint objectId)
        {
            if (!CanMoveOrDoActions())
                return;
            CallNetFunction(NetFuncPickupItem, FunctionReceivers.Server, new PackedUInt(objectId));
        }

        public virtual void RequestDropItem(ushort nonEquipIndex, short amount)
        {
            if (!CanMoveOrDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return;
            CallNetFunction(NetFuncDropItem, FunctionReceivers.Server, nonEquipIndex, amount);
        }

        public virtual void RequestEquipItem(ushort nonEquipIndex)
        {
            if (!CanMoveOrDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return;
            var characterItem = NonEquipItems[nonEquipIndex];
            var armorItem = characterItem.GetArmorItem();
            var weaponItem = characterItem.GetWeaponItem();
            var shieldItem = characterItem.GetShieldItem();
            if (weaponItem != null)
            {
                if (weaponItem.EquipType == WeaponItemEquipType.OneHandCanDual)
                {
                    var rightWeapon = EquipWeapons.rightHand.GetWeaponItem();
                    if (rightWeapon != null && rightWeapon.EquipType == WeaponItemEquipType.OneHandCanDual)
                        RequestEquipItem(nonEquipIndex, GameDataConst.EQUIP_POSITION_LEFT_HAND);
                    else
                        RequestEquipItem(nonEquipIndex, GameDataConst.EQUIP_POSITION_RIGHT_HAND);
                }
                else
                    RequestEquipItem(nonEquipIndex, GameDataConst.EQUIP_POSITION_RIGHT_HAND);
            }
            else if (shieldItem != null)
                RequestEquipItem(nonEquipIndex, GameDataConst.EQUIP_POSITION_LEFT_HAND);
            else if (armorItem != null)
                RequestEquipItem(nonEquipIndex, armorItem.EquipPosition);
        }

        public virtual void RequestEquipItem(ushort nonEquipIndex, string equipPosition)
        {
            if (!CanMoveOrDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return;
            CallNetFunction(NetFuncEquipItem, FunctionReceivers.Server, nonEquipIndex, equipPosition);
        }

        public virtual void RequestUnEquipItem(string equipPosition)
        {
            if (!CanMoveOrDoActions())
                return;
            CallNetFunction(NetFuncUnEquipItem, FunctionReceivers.Server, equipPosition);
        }

        public virtual void RequestOnDead()
        {
            CallNetFunction(NetFuncOnDead, ConnectionId);
        }

        public virtual void RequestOnRespawn()
        {
            CallNetFunction(NetFuncOnRespawn, ConnectionId);
        }

        public virtual void RequestOnLevelUp()
        {
            CallNetFunction(NetFuncOnLevelUp, ConnectionId);
        }
    }
}
