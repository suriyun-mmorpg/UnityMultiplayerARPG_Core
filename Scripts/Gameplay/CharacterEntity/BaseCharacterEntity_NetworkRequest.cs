using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public bool RequestPlayWeaponLaunchEffect(bool isLeftHand)
        {
            CallNetFunction(NetFuncPlayWeaponLaunchEffect, FunctionReceivers.All, isLeftHand);
            return true;
        }

        public bool RequestSetAimPosition(Vector3 aimPosition)
        {
            CallNetFunction(NetFuncSetAimPosition, DeliveryMethod.Sequenced, FunctionReceivers.Server, aimPosition);
            return true;
        }

        public bool RequestUnsetAimPosition()
        {
            CallNetFunction(NetFuncUnsetAimPosition, FunctionReceivers.Server);
            return true;
        }

        public bool RequestAttack(bool isLeftHand)
        {
            if (!CanAttack())
                return false;
            CallNetFunction(NetFuncAttackWithoutAimPosition, FunctionReceivers.Server, isLeftHand);
            return true;
        }

        public bool RequestAttack(bool isLeftHand, Vector3 aimPosition)
        {
            if (!CanAttack())
                return false;
            CallNetFunction(NetFuncAttackWithAimPosition, FunctionReceivers.Server, isLeftHand, aimPosition);
            return true;
        }

        public bool RequestUseSkill(int dataId, bool isLeftHand)
        {
            if (!CanUseSkill())
                return false;
            CallNetFunction(NetFuncUseSkillWithoutAimPosition, FunctionReceivers.Server, dataId, isLeftHand);
            return true;
        }

        public bool RequestUseSkill(int dataId, bool isLeftHand, Vector3 aimPosition)
        {
            if (!CanUseSkill())
                return false;
            CallNetFunction(NetFuncUseSkillWithAimPosition, FunctionReceivers.Server, dataId, isLeftHand, aimPosition);
            return true;
        }

        public bool RequestPlayActionAnimation(AnimActionType animActionType, int dataId, byte animationIndex)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncPlayActionAnimation, FunctionReceivers.All, (byte)animActionType, dataId, animationIndex);
            return true;
        }

        public bool RequestSkillCasting(int dataId, float duration)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncSkillCasting, FunctionReceivers.All, dataId, duration);
            return true;
        }

        public bool RequestSkillCastingInterrupted()
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncSkillCastingInterrupted, FunctionReceivers.All);
            return true;
        }

        public bool RequestPickupItem(uint objectId)
        {
            if (!CanDoActions())
                return false;
            CallNetFunction(NetFuncPickupItem, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestDropItem(short nonEquipIndex, short amount)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return false;
            CallNetFunction(NetFuncDropItem, FunctionReceivers.Server, nonEquipIndex, amount);
            return true;
        }

        public bool RequestEquipItem(short nonEquipIndex)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return false;
            CharacterItem characterItem = NonEquipItems[nonEquipIndex];
            Item armorItem = characterItem.GetArmorItem();
            Item weaponItem = characterItem.GetWeaponItem();
            Item shieldItem = characterItem.GetShieldItem();
            if (weaponItem != null)
            {
                if (weaponItem.EquipType == WeaponItemEquipType.OneHandCanDual)
                {
                    Item rightWeapon = EquipWeapons.rightHand.GetWeaponItem();
                    if (rightWeapon != null && rightWeapon.EquipType == WeaponItemEquipType.OneHandCanDual)
                        return RequestEquipItem(nonEquipIndex, (byte)InventoryType.EquipWeaponLeft, 0);
                    else
                        return RequestEquipItem(nonEquipIndex, (byte)InventoryType.EquipWeaponRight, 0);
                }
                else
                    return RequestEquipItem(nonEquipIndex, (byte)InventoryType.EquipWeaponRight, 0);
            }
            else if (shieldItem != null)
                return RequestEquipItem(nonEquipIndex, (byte)InventoryType.EquipWeaponLeft, 0);
            else if (armorItem != null)
                return RequestEquipItem(nonEquipIndex, (byte)InventoryType.EquipItems, (short)this.IndexOfEquipItemByEquipPosition(armorItem.EquipPosition));
            return false;
        }

        public bool RequestEquipItem(short nonEquipIndex, byte byteInventoryType, short oldEquipIndex)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return false;
            CallNetFunction(NetFuncEquipItem, FunctionReceivers.Server, nonEquipIndex, byteInventoryType, oldEquipIndex);
            return true;
        }

        public bool RequestUnEquipItem(byte byteInventoryType, short index)
        {
            if (!CanDoActions())
                return false;
            CallNetFunction(NetFuncUnEquipItem, FunctionReceivers.Server, byteInventoryType, index);
            return true;
        }

        public bool RequestOnDead()
        {
            CallNetFunction(NetFuncOnDead, ConnectionId);
            return true;
        }

        public bool RequestOnRespawn()
        {
            CallNetFunction(NetFuncOnRespawn, ConnectionId);
            return true;
        }

        public bool RequestOnLevelUp()
        {
            CallNetFunction(NetFuncOnLevelUp, ConnectionId);
            return true;
        }

        public bool RequestUnSummon(PackedUInt objectId)
        {
            CallNetFunction(NetFuncUnSummon, FunctionReceivers.Server, objectId);
            return true;
        }

        public bool RequestSwapOrMergeNonEquipItems(short index1, short index2)
        {
            CallNetFunction(NetFuncSwapOrMergeNonEquipItems, FunctionReceivers.Server, index1, index2);
            return true;
        }

        public bool RequestReload(bool isLeftHand)
        {
            CallNetFunction(NetFuncReload, FunctionReceivers.Server, isLeftHand);
            return true;
        }
    }
}
