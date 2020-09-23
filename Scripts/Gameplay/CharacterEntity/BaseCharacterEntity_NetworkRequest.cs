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

        public bool CallServerAttack(bool isLeftHand)
        {
            if (!ValidateRequestAttack(isLeftHand))
                return false;
            RPC(ServerAttack, isLeftHand);
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

        public bool CallServerUseSkill(int dataId, bool isLeftHand)
        {
            if (!ValidateRequestUseSKill(dataId, isLeftHand))
                return false;
            RPC(ServerUseSkill, dataId, isLeftHand);
            return true;
        }

        public bool CallServerUseSkill(int dataId, bool isLeftHand, Vector3 aimPosition)
        {
            if (!ValidateRequestUseSKill(dataId, isLeftHand))
                return false;
            RPC(ServerUseSkillWithAimPosition, dataId, isLeftHand, aimPosition);
            return true;
        }

        public bool CallAllPlayAttackAnimation(bool isLeftHand, byte animationIndex)
        {
            if (this.IsDead())
                return false;
            RPC(AllPlayAttackAnimation, isLeftHand, animationIndex);
            return true;
        }

        public bool CallAllPlaySkillAnimation(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel)
        {
            if (this.IsDead())
                return false;
            RPC(AllPlayUseSkillAnimation, isLeftHand, animationIndex, skillDataId, skillLevel);
            return true;
        }

        public bool CallAllPlaySkillAnimationWithAimPosition(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel, Vector3 aimPosition)
        {
            if (this.IsDead())
                return false;
            RPC(AllPlayUseSkillAnimationWithAimPosition, isLeftHand, animationIndex, skillDataId, skillLevel, aimPosition);
            return true;
        }

        public bool CallAllPlayReloadAnimation(bool isLeftHand, short reloadingAmmoAmount)
        {
            if (this.IsDead())
                return false;
            RPC(AllPlayReloadAnimation, isLeftHand, reloadingAmmoAmount);
            return true;
        }

        public bool CallServerSkillCastingInterrupt()
        {
            if (this.IsDead())
                return false;
            RPC(ServerSkillCastingInterrupt);
            return true;
        }

        public bool CallAllOnSkillCastingInterrupt()
        {
            if (this.IsDead())
                return false;
            RPC(AllOnSkillCastingInterrupt);
            return true;
        }

        public bool CallServerPickupItem(uint objectId)
        {
            if (!CanDoActions())
                return false;
            RPC(ServerPickupItem, objectId);
            CallAllPlayPickupAnimation();
            return true;
        }

        public bool CallServerDropItem(short nonEquipIndex, short amount)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return false;
            RPC(ServerDropItem, nonEquipIndex, amount);
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
                        return CallServerEquipWeapon(nonEquipIndex, EquipWeaponSet, true);
                    else
                        return CallServerEquipWeapon(nonEquipIndex, EquipWeaponSet, false);
                }
                else
                    return CallServerEquipWeapon(nonEquipIndex, EquipWeaponSet, false);
            }
            else if (equippingShieldItem != null)
            {
                // Shield can equip at left-hand only
                return CallServerEquipWeapon(nonEquipIndex, EquipWeaponSet, true);
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
                return CallServerEquipArmor(nonEquipIndex, equippingSlotIndex);
            }
            return false;
        }

        public bool RequestEquipItem(short nonEquipIndex, InventoryType inventoryType, byte equipSlotIndex)
        {
            switch (inventoryType)
            {
                case InventoryType.EquipItems:
                    return CallServerEquipArmor(nonEquipIndex, equipSlotIndex);
                case InventoryType.EquipWeaponRight:
                    return CallServerEquipWeapon(nonEquipIndex, equipSlotIndex, false);
                case InventoryType.EquipWeaponLeft:
                    return CallServerEquipWeapon(nonEquipIndex, equipSlotIndex, true);
            }
            return false;
        }

        private bool CallServerEquipWeapon(short nonEquipIndex, byte equipWeaponSet, bool isLeftHand)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return false;
            RPC(ServerEquipWeapon, nonEquipIndex, equipWeaponSet, isLeftHand);
            return true;
        }

        private bool CallServerEquipArmor(short nonEquipIndex, byte equipSlotIndex)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return false;
            RPC(ServerEquipArmor, nonEquipIndex, equipSlotIndex);
            return true;
        }

        public bool RequestUnEquipItem(InventoryType inventoryType, short equipItemIndex, byte equipWeaponSet)
        {
            switch (inventoryType)
            {
                case InventoryType.EquipItems:
                    return CallServerUnEquipArmor(equipItemIndex);
                case InventoryType.EquipWeaponRight:
                    return CallServerUnEquipWeapon(equipWeaponSet, false);
                case InventoryType.EquipWeaponLeft:
                    return CallServerUnEquipWeapon(equipWeaponSet, true);
            }
            return false;
        }

        private bool CallServerUnEquipWeapon(byte equipWeaponSet, bool isLeftHand)
        {
            if (!CanDoActions())
                return false;
            RPC(ServerUnEquipWeapon, equipWeaponSet, isLeftHand);
            return true;
        }

        private bool CallServerUnEquipArmor(short equipItemIndex)
        {
            if (!CanDoActions())
                return false;
            RPC(ServerUnEquipArmor, equipItemIndex);
            return true;
        }

        public bool CallOwnerOnDead()
        {
            RPC(TargetOnDead, ConnectionId);
            return true;
        }

        public bool CallOwnerOnRespawn()
        {
            RPC(TargetOnRespawn, ConnectionId);
            return true;
        }

        public bool CallOwnerOnLevelUp()
        {
            RPC(TargetOnLevelUp, ConnectionId);
            return true;
        }

        public bool CallServerUnSummon(uint objectId)
        {
            RPC(ServerUnSummon, objectId);
            return true;
        }

        public bool CallServerReload(bool isLeftHand)
        {
            if (!CanDoActions())
                return false;
            if (!isLeftHand && EquipWeapons.rightHand.IsAmmoFull())
                return false;
            if (isLeftHand && EquipWeapons.leftHand.IsAmmoFull())
                return false;
            RPC(ServerReload, isLeftHand);
            return true;
        }

        public bool CallServerSwitchEquipWeaponSet(byte equipWeaponSet)
        {
            if (!CanDoActions() || EquipWeaponSet == equipWeaponSet)
                return false;
            if (equipWeaponSet >= CurrentGameInstance.maxEquipWeaponSet)
                equipWeaponSet = 0;
            RPC(ServerSwitchEquipWeaponSet, equipWeaponSet);
            return true;
        }
    }
}
