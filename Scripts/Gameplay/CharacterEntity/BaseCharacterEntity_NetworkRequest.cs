using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public bool ValidateRequestAttack(bool isLeftHand)
        {
            if (!CanAttack())
                return false;

            float time = Time.unscaledTime;
            if (time - lastActionTime < ACTION_DELAY)
                return false;
            lastActionTime = time;

            CharacterItem weapon = this.GetAvailableWeapon(ref isLeftHand);
            if (!ValidateAmmo(weapon))
            {
                QueueGameMessage(GameMessage.Type.NoAmmo);
                return false;
            }
            return true;
        }

        public bool RequestAttack(bool isLeftHand)
        {
            if (!ValidateRequestAttack(isLeftHand))
                return false;
            CallNetFunction(ServerAttack, FunctionReceivers.Server, isLeftHand);
            return true;
        }

        public bool ValidateRequestUseSKill(int dataId, bool isLeftHand)
        {
            if (!CanUseSkill())
                return false;

            float time = Time.unscaledTime;
            if (time - lastActionTime < ACTION_DELAY)
                return false;
            lastActionTime = time;

            BaseSkill skill;
            short skillLevel;
            if (!GameInstance.Skills.TryGetValue(dataId, out skill) ||
                !this.GetCaches().Skills.TryGetValue(skill, out skillLevel))
                return false;

            GameMessage.Type gameMessageType;
            if (!skill.CanUse(this, skillLevel, isLeftHand, out gameMessageType))
            {
                QueueGameMessage(gameMessageType);
                return false;
            }
            return true;
        }

        public bool RequestUseSkill(int dataId, bool isLeftHand)
        {
            if (!ValidateRequestUseSKill(dataId, isLeftHand))
                return false;
            CallNetFunction(ServerUseSkill, FunctionReceivers.Server, dataId, isLeftHand);
            return true;
        }

        public bool RequestUseSkill(int dataId, bool isLeftHand, Vector3 aimPosition)
        {
            if (!ValidateRequestUseSKill(dataId, isLeftHand))
                return false;
            CallNetFunction(ServerUseSkillWithAimPosition, FunctionReceivers.Server, dataId, isLeftHand, aimPosition);
            return true;
        }

        public bool RequestPlayAttackAnimation(bool isLeftHand, byte animationIndex)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(NetFuncPlayAttack, FunctionReceivers.All, isLeftHand, animationIndex);
            return true;
        }

        public bool RequestPlaySkillAnimation(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(NetFuncPlayUseSkill, FunctionReceivers.All, isLeftHand, animationIndex, skillDataId, skillLevel);
            return true;
        }

        public bool RequestPlaySkillAnimationWithAimPosition(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel, Vector3 aimPosition)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(NetFuncPlayUseSkillWithAimPosition, FunctionReceivers.All, isLeftHand, animationIndex, skillDataId, skillLevel, aimPosition);
            return true;
        }

        public bool RequestPlayReloadAnimation(bool isLeftHand, short reloadingAmmoAmount)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(NetFuncPlayReload, FunctionReceivers.All, isLeftHand, reloadingAmmoAmount);
            return true;
        }

        public bool RequestSkillCastingInterrupt()
        {
            if (this.IsDead())
                return false;
            CallNetFunction(ServerSkillCastingInterrupt, FunctionReceivers.Server);
            return true;
        }

        public bool RequestSkillCastingInterrupted()
        {
            if (this.IsDead())
                return false;
            CallNetFunction(NetFuncSkillCastingInterrupted, FunctionReceivers.All);
            return true;
        }

        public bool RequestPickupItem(uint objectId)
        {
            if (!CanDoActions())
                return false;
            CallNetFunction(ServerPickupItem, FunctionReceivers.Server, new PackedUInt(objectId));
            TriggerPickup();
            return true;
        }

        public bool RequestDropItem(short nonEquipIndex, short amount)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return false;
            CallNetFunction(ServerDropItem, FunctionReceivers.Server, nonEquipIndex, amount);
            return true;
        }

        public bool RequestEquipItem(short nonEquipIndex)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return false;

            CharacterItem equippingItem = NonEquipItems[nonEquipIndex];
            IArmorItem equippingArmorItem = equippingItem.GetArmorItem();
            IWeaponItem equippingWeaponItem = equippingItem.GetWeaponItem();
            IShieldItem equippingShieldItem = equippingItem.GetShieldItem();
            if (equippingWeaponItem != null)
            {
                if (equippingWeaponItem.EquipType == WeaponItemEquipType.OneHandCanDual)
                {
                    IWeaponItem rightWeapon = EquipWeapons.GetRightHandWeaponItem();
                    // Equip at left-hand if able to do it
                    if (rightWeapon != null && rightWeapon.EquipType == WeaponItemEquipType.OneHandCanDual)
                        return RequestEquipWeapon(nonEquipIndex, EquipWeaponSet, true);
                    else
                        return RequestEquipWeapon(nonEquipIndex, EquipWeaponSet, false);
                }
                else
                    return RequestEquipWeapon(nonEquipIndex, EquipWeaponSet, false);
            }
            else if (equippingShieldItem != null)
            {
                // Shield can equip at left-hand only
                return RequestEquipWeapon(nonEquipIndex, EquipWeaponSet, true);
            }
            else if (equippingArmorItem != null)
            {
                // Find equip slot index
                // Example: if there is 2 ring slots
                // If first ring slot is empty, equip to first ring slot
                // The if first ring slot is not empty, equip to second ring slot
                // Do not equip to third ring slot because it's not allowed to do that
                byte equippingSlotIndex = (byte)(equippingArmorItem.ArmorType.equippableSlots - 1);
                bool[] equippedSlots = new bool[equippingArmorItem.ArmorType.equippableSlots];
                CharacterItem equippedItem;
                for (short i = 0; i < EquipItems.Count; ++i)
                {
                    equippedItem = EquipItems[i];
                    // If equipped item is same armor type, find which slot it is equipped
                    if (equippedItem.GetArmorItem().ArmorType == equippingArmorItem.ArmorType)
                        equippedSlots[equippedItem.equipSlotIndex] = true;
                }
                // Find free slot
                for (byte i = 0; i < equippedSlots.Length; ++i)
                {
                    if (!equippedSlots[i])
                    {
                        equippingSlotIndex = i;
                        break;
                    }
                }
                return RequestEquipArmor(nonEquipIndex, equippingSlotIndex);
            }
            return false;
        }

        public bool RequestEquipItem(short nonEquipIndex, InventoryType inventoryType, byte equipSlotIndex)
        {
            switch (inventoryType)
            {
                case InventoryType.EquipItems:
                    return RequestEquipArmor(nonEquipIndex, equipSlotIndex);
                case InventoryType.EquipWeaponRight:
                    return RequestEquipWeapon(nonEquipIndex, equipSlotIndex, false);
                case InventoryType.EquipWeaponLeft:
                    return RequestEquipWeapon(nonEquipIndex, equipSlotIndex, true);
            }
            return false;
        }

        private bool RequestEquipWeapon(short nonEquipIndex, byte equipWeaponSet, bool isLeftHand)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return false;
            CallNetFunction(ServerEquipWeapon, FunctionReceivers.Server, nonEquipIndex, equipWeaponSet, isLeftHand);
            return true;
        }

        private bool RequestEquipArmor(short nonEquipIndex, byte equipSlotIndex)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return false;
            CallNetFunction(ServerEquipArmor, FunctionReceivers.Server, nonEquipIndex, equipSlotIndex);
            return true;
        }

        public bool RequestUnEquipItem(InventoryType inventoryType, short equipItemIndex, byte equipWeaponSet)
        {
            switch (inventoryType)
            {
                case InventoryType.EquipItems:
                    return RequestUnEquipArmor(equipItemIndex);
                case InventoryType.EquipWeaponRight:
                    return RequestUnEquipWeapon(equipWeaponSet, false);
                case InventoryType.EquipWeaponLeft:
                    return RequestUnEquipWeapon(equipWeaponSet, true);
            }
            return false;
        }

        private bool RequestUnEquipWeapon(byte equipWeaponSet, bool isLeftHand)
        {
            if (!CanDoActions())
                return false;
            CallNetFunction(ServerUnEquipWeapon, FunctionReceivers.Server, equipWeaponSet, isLeftHand);
            return true;
        }

        private bool RequestUnEquipArmor(short equipItemIndex)
        {
            if (!CanDoActions())
                return false;
            CallNetFunction(ServerUnEquipArmor, FunctionReceivers.Server, equipItemIndex);
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
            CallNetFunction(ServerUnSummon, FunctionReceivers.Server, objectId);
            return true;
        }

        public bool RequestReload(bool isLeftHand)
        {
            CallNetFunction(ServerReload, FunctionReceivers.Server, isLeftHand);
            return true;
        }

        public bool RequestSwitchEquipWeaponSet(byte equipWeaponSet)
        {
            if (!CanDoActions() || EquipWeaponSet == equipWeaponSet)
                return false;
            if (equipWeaponSet >= CurrentGameInstance.maxEquipWeaponSet)
                equipWeaponSet = 0;
            CallNetFunction(ServerSwitchEquipWeaponSet, FunctionReceivers.Server, equipWeaponSet);
            return true;
        }
    }
}
