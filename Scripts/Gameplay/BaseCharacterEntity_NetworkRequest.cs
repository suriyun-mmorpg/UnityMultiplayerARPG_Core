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
            if (IsDead() || IsPlayingActionAnimation())
                return;
            CallNetFunction("Attack", FunctionReceivers.Server);
        }

        public virtual void RequestUseSkill(Vector3 position, int skillIndex)
        {
            if (IsDead() ||
                IsPlayingActionAnimation() ||
                skillIndex < 0 ||
                skillIndex >= Skills.Count ||
                !Skills[skillIndex].CanUse(this))
                return;
            CallNetFunction("UseSkill", FunctionReceivers.Server, position, skillIndex);
        }

        public virtual void RequestUseItem(int itemIndex)
        {
            if (IsDead())
                return;
            CallNetFunction("UseItem", FunctionReceivers.Server, itemIndex);
        }

        public virtual void RequestPlayActionAnimation(int actionId, AnimActionType animActionType)
        {
            if (IsDead() || actionId < 0)
                return;
            CallNetFunction("PlayActionAnimation", FunctionReceivers.All, actionId, animActionType);
        }

        public virtual void RequestPlayEffect(int effectId)
        {
            if (effectId < 0)
                return;
            CallNetFunction("PlayEffect", FunctionReceivers.All, effectId);
        }

        public virtual void RequestPickupItem(uint objectId)
        {
            if (IsDead() || IsPlayingActionAnimation())
                return;
            CallNetFunction("PickupItem", FunctionReceivers.Server, objectId);
        }

        public virtual void RequestDropItem(int nonEquipIndex, short amount)
        {
            if (IsDead() ||
                IsPlayingActionAnimation() ||
                nonEquipIndex < 0 ||
                nonEquipIndex >= NonEquipItems.Count)
                return;
            CallNetFunction("DropItem", FunctionReceivers.Server, nonEquipIndex, amount);
        }

        public virtual void RequestEquipItem(int nonEquipIndex)
        {
            if (IsDead() ||
                IsPlayingActionAnimation() ||
                nonEquipIndex < 0 ||
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

        public virtual void RequestEquipItem(int nonEquipIndex, string equipPosition)
        {
            if (IsDead() ||
                IsPlayingActionAnimation() ||
                nonEquipIndex < 0 ||
                nonEquipIndex >= NonEquipItems.Count)
                return;
            CallNetFunction("EquipItem", FunctionReceivers.Server, nonEquipIndex, equipPosition);
        }

        public virtual void RequestUnEquipItem(string equipPosition)
        {
            if (IsDead() || IsPlayingActionAnimation())
                return;
            CallNetFunction("UnEquipItem", FunctionReceivers.Server, equipPosition);
        }

        public virtual void RequestOnDead(bool isInitialize)
        {
            CallNetFunction("OnDead", FunctionReceivers.All, isInitialize);
        }

        public virtual void RequestOnRespawn(bool isInitialize)
        {
            CallNetFunction("OnRespawn", FunctionReceivers.All, isInitialize);
        }

        public virtual void RequestOnLevelUp()
        {
            CallNetFunction("OnLevelUp", FunctionReceivers.All);
        }
    }
}
