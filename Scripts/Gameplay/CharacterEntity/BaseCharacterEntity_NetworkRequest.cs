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
            CallNetFunction("Attack", FunctionReceivers.Server);
        }

        public virtual void RequestUseSkill(Vector3 position, int dataId)
        {
            if (!CanMoveOrDoActions())
                return;
            CallNetFunction("UseSkill", FunctionReceivers.Server, position, dataId);
        }

        public virtual void RequestUseItem(int dataId)
        {
            if (IsDead())
                return;
            CallNetFunction("UseItem", FunctionReceivers.Server, dataId);
        }

        public virtual void RequestPlayActionAnimation(AnimActionType animActionType, int dataId, byte index)
        {
            if (IsDead())
                return;
            CallNetFunction("PlayActionAnimation", FunctionReceivers.All, animActionType, dataId, index);
        }

        public virtual void RequestPickupItem(uint objectId)
        {
            if (!CanMoveOrDoActions())
                return;
            CallNetFunction("PickupItem", FunctionReceivers.Server, objectId);
        }

        public virtual void RequestDropItem(ushort nonEquipIndex, short amount)
        {
            if (!CanMoveOrDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return;
            CallNetFunction("DropItem", FunctionReceivers.Server, nonEquipIndex, amount);
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
            CallNetFunction("EquipItem", FunctionReceivers.Server, nonEquipIndex, equipPosition);
        }

        public virtual void RequestUnEquipItem(string equipPosition)
        {
            if (!CanMoveOrDoActions())
                return;
            CallNetFunction("UnEquipItem", FunctionReceivers.Server, equipPosition);
        }

        public virtual void RequestOnDead()
        {
            CallNetFunction("OnDead", ConnectionId);
        }

        public virtual void RequestOnRespawn()
        {
            CallNetFunction("OnRespawn", ConnectionId);
        }

        public virtual void RequestOnLevelUp()
        {
            CallNetFunction("OnLevelUp", ConnectionId);
        }
    }
}
