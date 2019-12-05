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
            
            CharacterItem weapon = this.GetAvailableWeapon(ref isLeftHand);
            if (!ValidateAmmo(weapon))
            {
                if (Time.unscaledTime - lastCombatantErrorTime >= COMBATANT_MESSAGE_DELAY)
                {
                    if (!IsOwnerClient)
                        return false;

                    lastCombatantErrorTime = Time.unscaledTime;
                    gameManager.ClientReceiveGameMessage(new GameMessage() { type = GameMessage.Type.NoAmmo });
                }
                return false;
            }

            return true;
        }

        public bool RequestAttack(bool isLeftHand, Vector3 aimPosition)
        {
            if (!ValidateRequestAttack(isLeftHand))
                return false;
            CallNetFunction(NetFuncAttack, FunctionReceivers.Server, isLeftHand, aimPosition);
            return true;
        }

        public bool ValidateRequestUseSKill(int dataId, bool isLeftHand)
        {
            if (!CanUseSkill())
                return false;

            BaseSkill skill;
            short skillLevel;
            if (!GameInstance.Skills.TryGetValue(dataId, out skill) ||
                !this.GetCaches().Skills.TryGetValue(skill, out skillLevel))
                return false;

            float currentTime = Time.unscaledTime;
            if (!requestUseSkillErrorTime.ContainsKey(dataId))
                requestUseSkillErrorTime[dataId] = currentTime;

            GameMessage.Type gameMessageType;
            if (!skill.CanUse(this, skillLevel, isLeftHand, out gameMessageType))
            {
                if (!IsOwnerClient)
                    return false;

                if (Time.unscaledTime - requestUseSkillErrorTime[dataId] >= COMBATANT_MESSAGE_DELAY)
                {
                    requestUseSkillErrorTime[dataId] = currentTime;
                    gameManager.ClientReceiveGameMessage(new GameMessage() { type = gameMessageType });
                }
                return false;
            }
            
            return true;
        }

        public bool RequestUseSkill(int dataId, bool isLeftHand, Vector3 aimPosition)
        {
            if (!ValidateRequestUseSKill(dataId, isLeftHand))
                return false;
            CallNetFunction(NetFuncUseSkill, FunctionReceivers.Server, dataId, isLeftHand, aimPosition);
            return true;
        }

        public bool RequestPlayAttackAnimation(bool isLeftHand, byte animationIndex, Vector3 aimPosition)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncPlayAttack, FunctionReceivers.All, isLeftHand, animationIndex, aimPosition);
            return true;
        }

        public bool RequestPlaySkillAnimation(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel, Vector3 aimPosition)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncPlayUseSkill, FunctionReceivers.All, isLeftHand, animationIndex, skillDataId, skillLevel, aimPosition);
            return true;
        }

        public bool RequestPlayReloadAnimation(bool isLeftHand)
        {
            if (IsDead())
                return false;
            CallNetFunction(NetFuncPlayReload, FunctionReceivers.All, isLeftHand);
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

            CharacterItem equippingItem = NonEquipItems[nonEquipIndex];
            Item equippingArmorItem = equippingItem.GetArmorItem();
            Item equippingWeaponItem = equippingItem.GetWeaponItem();
            Item equippingShieldItem = equippingItem.GetShieldItem();
            if (equippingWeaponItem != null)
            {
                if (equippingWeaponItem.EquipType == WeaponItemEquipType.OneHandCanDual)
                {
                    Item rightWeapon = EquipWeapons.GetRightHandWeaponItem();
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
                    if (equippedItem.GetItem().ArmorType == equippingArmorItem.ArmorType)
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
            CallNetFunction(NetFuncEquipWeapon, FunctionReceivers.Server, nonEquipIndex, equipWeaponSet, isLeftHand);
            return true;
        }

        private bool RequestEquipArmor(short nonEquipIndex, byte equipSlotIndex)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return false;
            CallNetFunction(NetFuncEquipArmor, FunctionReceivers.Server, nonEquipIndex, equipSlotIndex);
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
            CallNetFunction(NetFuncUnEquipWeapon, FunctionReceivers.Server, equipWeaponSet, isLeftHand);
            return true;
        }

        private bool RequestUnEquipArmor(short equipItemIndex)
        {
            if (!CanDoActions())
                return false;
            CallNetFunction(NetFuncUnEquipArmor, FunctionReceivers.Server, equipItemIndex);
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

        public bool RequestReload(bool isLeftHand)
        {
            CallNetFunction(NetFuncReload, FunctionReceivers.Server, isLeftHand);
            return true;
        }

        public bool RequestSwitchEquipWeaponSet(byte equipWeaponSet)
        {
            if (!CanDoActions() || EquipWeaponSet == equipWeaponSet)
                return false;
            if (equipWeaponSet >= gameInstance.maxEquipWeaponSet)
                equipWeaponSet = 0;
            CallNetFunction(NetFuncSwitchEquipWeaponSet, FunctionReceivers.Server, equipWeaponSet);
            return true;
        }
    }
}
