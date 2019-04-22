using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {

        public virtual void RequestSetAimPosition(Vector3 aimPosition)
        {
            CallNetFunction(NetFuncSetAimPosition, DeliveryMethod.Sequenced, FunctionReceivers.Server, aimPosition);
        }

        public virtual void RequestUnsetAimPosition()
        {
            CallNetFunction(NetFuncUnsetAimPosition, FunctionReceivers.Server);
        }

        public virtual void RequestAttack()
        {
            if (!CanAttack())
                return;
            CallNetFunction(NetFuncAttackWithoutAimPosition, FunctionReceivers.Server);
        }

        public virtual void RequestAttack(Vector3 aimPosition)
        {
            if (!CanAttack())
                return;
            CallNetFunction(NetFuncAttackWithAimPosition, FunctionReceivers.Server, aimPosition);
        }

        public virtual void RequestUseSkill(int dataId)
        {
            if (!CanUseSkill())
                return;
            CallNetFunction(NetFuncUseSkillWithoutAimPosition, FunctionReceivers.Server, dataId);
        }

        public virtual void RequestUseSkill(int dataId, Vector3 aimPosition)
        {
            if (!CanUseSkill())
                return;
            CallNetFunction(NetFuncUseSkillWithAimPosition, FunctionReceivers.Server, dataId, aimPosition);
        }

        public virtual void RequestUseItem(short index)
        {
            if (!CanUseItem())
                return;
            CallNetFunction(NetFuncUseItem, FunctionReceivers.Server, index);
        }

        public virtual void RequestPlayActionAnimation(AnimActionType animActionType, int dataId, byte animationIndex)
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncPlayActionAnimation, FunctionReceivers.All, (byte)animActionType, dataId, animationIndex);
        }

        public virtual void RequestSkillCasting(int dataId, float duration)
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncSkillCasting, FunctionReceivers.All, dataId, duration);
        }

        public virtual void RequestSkillCastingInterrupted()
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncSkillCastingInterrupted, FunctionReceivers.All);
        }

        public virtual void RequestPickupItem(uint objectId)
        {
            if (!CanDoActions())
                return;
            CallNetFunction(NetFuncPickupItem, FunctionReceivers.Server, new PackedUInt(objectId));
        }

        public virtual void RequestDropItem(short nonEquipIndex, short amount)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return;
            CallNetFunction(NetFuncDropItem, FunctionReceivers.Server, nonEquipIndex, amount);
        }

        public virtual void RequestReload()
        {
            if (!CanDoActions())
                return;
            CallNetFunction(NetFuncReload, FunctionReceivers.Server);
        }

        public virtual void RequestEquipItem(short nonEquipIndex)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return;
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
                        RequestEquipItem(nonEquipIndex, (byte)InventoryType.EquipWeaponLeft, 0);
                    else
                        RequestEquipItem(nonEquipIndex, (byte)InventoryType.EquipWeaponRight, 0);
                }
                else
                    RequestEquipItem(nonEquipIndex, (byte)InventoryType.EquipWeaponRight, 0);
            }
            else if (shieldItem != null)
                RequestEquipItem(nonEquipIndex, (byte)InventoryType.EquipWeaponLeft, 0);
            else if (armorItem != null)
                RequestEquipItem(nonEquipIndex, (byte)InventoryType.EquipItems, (short)this.IndexOfEquipItem(armorItem.EquipPosition));
        }

        public virtual void RequestEquipItem(short nonEquipIndex, byte byteInventoryType, short oldEquipIndex)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return;
            CallNetFunction(NetFuncEquipItem, FunctionReceivers.Server, nonEquipIndex, byteInventoryType, oldEquipIndex);
        }

        public virtual void RequestUnEquipItem(byte byteInventoryType, short index)
        {
            if (!CanDoActions())
                return;
            CallNetFunction(NetFuncUnEquipItem, FunctionReceivers.Server, byteInventoryType, index);
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

        public virtual void RequestUnSummon(PackedUInt objectId)
        {
            CallNetFunction(NetFuncUnSummon, FunctionReceivers.Server, objectId);
        }

        public virtual void RequestSwapOrMergeNonEquipItems(short index1, short index2)
        {
            CallNetFunction(NetFuncSwapOrMergeNonEquipItems, FunctionReceivers.Server, index1, index2);
        }

        public virtual void RequestReload(bool isLeftHand)
        {
            CallNetFunction(NetFuncReload, FunctionReceivers.Server, isLeftHand);
        }
    }
}
