using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        protected void NetFuncPlayAttack(bool isLeftHand, byte animationIndex)
        {
            AttackRoutine(isLeftHand, animationIndex).Forget();
        }

        protected void NetFuncPlayUseSkill(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel)
        {
            PlayUseSkillFunction(isLeftHand, animationIndex, skillDataId, skillLevel, null);
        }

        protected void NetFuncPlayUseSkillWithAimPosition(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel, Vector3 aimPosition)
        {
            PlayUseSkillFunction(isLeftHand, animationIndex, skillDataId, skillLevel, aimPosition);
        }

        protected virtual void PlayUseSkillFunction(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel, Vector3? aimPosition)
        {
            BaseSkill skill;
            if (GameInstance.Skills.TryGetValue(skillDataId, out skill) && skillLevel > 0)
            {
                UseSkillRoutine(isLeftHand, animationIndex, skill, skillLevel, aimPosition).Forget();
            }
            else
            {
                ClearActionStates();
            }
        }

        protected void NetFuncPlayReload(bool isLeftHand, short reloadingAmmoAmount)
        {
            ReloadRoutine(isLeftHand, reloadingAmmoAmount).Forget();
        }

        /// <summary>
        /// This will be called at server to order character to pickup items
        /// </summary>
        /// <param name="objectId"></param>
        [ServerRpc]
        protected virtual void ServerPickupItem(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            ItemDropEntity itemDropEntity = null;
            if (!Manager.TryGetEntityByObjectId(objectId, out itemDropEntity))
                return;

            if (Vector3.Distance(CacheTransform.position, itemDropEntity.CacheTransform.position) > CurrentGameInstance.pickUpItemDistance + 5f)
                return;

            if (!itemDropEntity.IsAbleToLoot(this))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.NotAbleToLoot);
                return;
            }

            if (this.IncreasingItemsWillOverwhelming(itemDropEntity.DropItems))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotCarryAnymore);
                return;
            }

            this.IncreaseItems(itemDropEntity.DropItems, (dataId, level, amount) =>
            {
                CurrentGameManager.SendNotifyRewardItem(ConnectionId, dataId, amount);
            });
            this.FillEmptySlots();
            itemDropEntity.PickedUp();
#endif
        }

        /// <summary>
        /// This will be called at server to order character to drop items
        /// </summary>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        [ServerRpc]
        protected virtual void ServerDropItem(short index, short amount)
        {
#if !CLIENT_BUILD
            if (!CanDoActions() ||
                index >= nonEquipItems.Count)
                return;

            CharacterItem nonEquipItem = nonEquipItems[index];
            if (nonEquipItem.IsEmptySlot() || amount > nonEquipItem.amount)
                return;

            if (!this.DecreaseItemsByIndex(index, amount))
                return;

            this.FillEmptySlots();
            // Drop item to the ground
            CharacterItem dropData = nonEquipItem.Clone();
            dropData.amount = amount;
            ItemDropEntity.DropItem(this, dropData, new uint[] { ObjectId });
#endif
        }

        /// <summary>
        /// This will be called at server to order character to equip weapon or shield
        /// </summary>
        /// <param name="nonEquipIndex"></param>
        /// <param name="equipWeaponSet"></param>
        /// <param name="isLeftHand"></param>
        [ServerRpc]
        protected virtual void ServerEquipWeapon(short nonEquipIndex, byte equipWeaponSet, bool isLeftHand)
        {
#if !CLIENT_BUILD
            if (!CanDoActions() ||
                nonEquipIndex >= nonEquipItems.Count)
                return;

            CharacterItem equippingItem = nonEquipItems[nonEquipIndex];

            GameMessage.Type gameMessageType;
            bool shouldUnequipRightHand;
            bool shouldUnequipLeftHand;
            if (!CanEquipWeapon(equippingItem, equipWeaponSet, isLeftHand, out gameMessageType, out shouldUnequipRightHand, out shouldUnequipLeftHand))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                return;
            }

            int unEquipCount = -1;
            if (shouldUnequipRightHand)
                ++unEquipCount;
            if (shouldUnequipLeftHand)
                ++unEquipCount;

            if (this.UnEquipItemWillOverwhelming(unEquipCount))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotCarryAnymore);
                return;
            }

            int unEquippedIndexRightHand = -1;
            if (shouldUnequipRightHand)
            {
                if (!UnEquipWeapon(equipWeaponSet, false, true, out gameMessageType, out unEquippedIndexRightHand))
                {
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    return;
                }
            }
            int unEquippedIndexLeftHand = -1;
            if (shouldUnequipLeftHand)
            {
                if (!UnEquipWeapon(equipWeaponSet, true, true, out gameMessageType, out unEquippedIndexLeftHand))
                {
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    return;
                }
            }

            // Equipping items
            this.FillWeaponSetsIfNeeded(equipWeaponSet);
            EquipWeapons tempEquipWeapons = SelectableWeaponSets[equipWeaponSet];
            if (isLeftHand)
            {
                equippingItem.equipSlotIndex = equipWeaponSet;
                tempEquipWeapons.leftHand = equippingItem;
                SelectableWeaponSets[equipWeaponSet] = tempEquipWeapons;
            }
            else
            {
                equippingItem.equipSlotIndex = equipWeaponSet;
                tempEquipWeapons.rightHand = equippingItem;
                SelectableWeaponSets[equipWeaponSet] = tempEquipWeapons;
            }
            // Update inventory
            if (unEquippedIndexRightHand >= 0 && unEquippedIndexLeftHand >= 0)
            {
                // Swap with equipped item
                NonEquipItems[nonEquipIndex] = NonEquipItems[unEquippedIndexRightHand];
                if (CurrentGameInstance.IsLimitInventorySlot)
                    NonEquipItems[unEquippedIndexRightHand] = CharacterItem.Empty;
                else
                    NonEquipItems.RemoveAt(unEquippedIndexRightHand);
                // Find empty slot for unequipped left-hand weapon to swap with empty slot
                if (CurrentGameInstance.IsLimitInventorySlot)
                {
                    NonEquipItems[this.IndexOfEmptyNonEquipItemSlot()] = NonEquipItems[unEquippedIndexLeftHand];
                    NonEquipItems[unEquippedIndexLeftHand] = CharacterItem.Empty;
                }
            }
            else if (unEquippedIndexRightHand >= 0)
            {
                // Swap with equipped item
                NonEquipItems[nonEquipIndex] = NonEquipItems[unEquippedIndexRightHand];
                if (CurrentGameInstance.IsLimitInventorySlot)
                    NonEquipItems[unEquippedIndexRightHand] = CharacterItem.Empty;
                else
                    NonEquipItems.RemoveAt(unEquippedIndexRightHand);
            }
            else if (unEquippedIndexLeftHand >= 0)
            {
                // Swap with equipped item
                NonEquipItems[nonEquipIndex] = NonEquipItems[unEquippedIndexLeftHand];
                if (CurrentGameInstance.IsLimitInventorySlot)
                    NonEquipItems[unEquippedIndexLeftHand] = CharacterItem.Empty;
                else
                    NonEquipItems.RemoveAt(unEquippedIndexLeftHand);
            }
            else
            {
                // Remove equipped item
                if (CurrentGameInstance.IsLimitInventorySlot)
                    NonEquipItems[nonEquipIndex] = CharacterItem.Empty;
                else
                    NonEquipItems.RemoveAt(nonEquipIndex);
            }
            this.FillEmptySlots(true);
#endif
        }

        /// <summary>
        /// This will be called at server to order character to equip armor
        /// </summary>
        /// <param name="nonEquipIndex"></param>
        /// <param name="equipSlotIndex"></param>
        [ServerRpc]
        protected virtual void ServerEquipArmor(short nonEquipIndex, byte equipSlotIndex)
        {
#if !CLIENT_BUILD
            if (!CanDoActions() ||
                nonEquipIndex >= nonEquipItems.Count)
                return;

            CharacterItem equippingItem = nonEquipItems[nonEquipIndex];

            GameMessage.Type gameMessageType;
            int unEquippingIndex = -1;
            if (!CanEquipItem(equippingItem, equipSlotIndex, out gameMessageType, out unEquippingIndex))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                return;
            }

            int unEquippedIndex = -1;
            if (unEquippingIndex >= 0 && !UnEquipArmor(unEquippingIndex, true, out gameMessageType, out unEquippedIndex))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                return;
            }

            // Can equip the item when there is no equipped item or able to unequip the equipped item
            equippingItem.equipSlotIndex = equipSlotIndex;
            equipItems.Add(equippingItem);
            // Update inventory
            if (unEquippedIndex >= 0)
            {
                // Swap with equipped item
                NonEquipItems[nonEquipIndex] = NonEquipItems[unEquippedIndex];
                if (CurrentGameInstance.IsLimitInventorySlot)
                    NonEquipItems[unEquippedIndex] = CharacterItem.Empty;
                else
                    NonEquipItems.RemoveAt(unEquippedIndex);
            }
            else
            {
                // Remove equipped item
                if (CurrentGameInstance.IsLimitInventorySlot)
                    NonEquipItems[nonEquipIndex] = CharacterItem.Empty;
                else
                    NonEquipItems.RemoveAt(nonEquipIndex);
            }
            this.FillEmptySlots(true);
#endif
        }

        [ServerRpc]
        protected virtual void ServerUnEquipWeapon(byte equipWeaponSet, bool isLeftHand)
        {
#if !CLIENT_BUILD
            GameMessage.Type gameMessageType;
            int unEquippedIndex;
            if (!UnEquipWeapon(equipWeaponSet, isLeftHand, false, out gameMessageType, out unEquippedIndex))
            {
                // Cannot unequip weapon, send reasons to client
                CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
            }
#endif
        }

        protected bool UnEquipWeapon(byte equipWeaponSet, bool isLeftHand, bool doNotValidate, out GameMessage.Type gameMessageType, out int unEquippedIndex)
        {
            gameMessageType = GameMessage.Type.None;
            unEquippedIndex = -1;
            if (!CanDoActions())
                return false;

            this.FillWeaponSetsIfNeeded(equipWeaponSet);
            EquipWeapons tempEquipWeapons = SelectableWeaponSets[equipWeaponSet];
            CharacterItem unEquipItem = CharacterItem.Empty;

            if (isLeftHand)
            {
                // Unequip left-hand weapon
                unEquipItem = tempEquipWeapons.leftHand;
                if (!doNotValidate && unEquipItem.NotEmptySlot() &&
                    this.UnEquipItemWillOverwhelming())
                {
                    gameMessageType = GameMessage.Type.CannotCarryAnymore;
                    return false;
                }
                tempEquipWeapons.leftHand = CharacterItem.Empty;
                SelectableWeaponSets[equipWeaponSet] = tempEquipWeapons;
            }
            else
            {
                // Unequip right-hand weapon
                unEquipItem = tempEquipWeapons.rightHand;
                if (!doNotValidate && unEquipItem.NotEmptySlot() &&
                    this.UnEquipItemWillOverwhelming())
                {
                    gameMessageType = GameMessage.Type.CannotCarryAnymore;
                    return false;
                }
                tempEquipWeapons.rightHand = CharacterItem.Empty;
                SelectableWeaponSets[equipWeaponSet] = tempEquipWeapons;
            }

            if (unEquipItem.NotEmptySlot())
            {
                this.AddOrSetNonEquipItems(unEquipItem, out unEquippedIndex);
                this.FillEmptySlots(true);
            }

            return true;
        }

        /// <summary>
        /// This will be called at server to order character to unequip equipments
        /// </summary>
        /// <param name="index"></param>
        [ServerRpc]
        protected virtual void ServerUnEquipArmor(short index)
        {
#if !CLIENT_BUILD
            GameMessage.Type gameMessageType;
            int unEquippedIndex;
            if (!UnEquipArmor(index, false, out gameMessageType, out unEquippedIndex))
            {
                // Cannot unequip weapon, send reasons to client
                CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
            }
#endif
        }

        protected bool UnEquipArmor(int index, bool doNotValidate, out GameMessage.Type gameMessageType, out int unEquippedIndex)
        {
            gameMessageType = GameMessage.Type.None;
            unEquippedIndex = -1;
            if (!CanDoActions() || index >= equipItems.Count)
                return false;

            EquipWeapons tempEquipWeapons = EquipWeapons;
            CharacterItem unEquipItem = equipItems[index];
            if (!doNotValidate && unEquipItem.NotEmptySlot() &&
                this.UnEquipItemWillOverwhelming())
            {
                gameMessageType = GameMessage.Type.CannotCarryAnymore;
                return false;
            }
            equipItems.RemoveAt(index);

            if (unEquipItem.NotEmptySlot())
            {
                this.AddOrSetNonEquipItems(unEquipItem, out unEquippedIndex);
                this.FillEmptySlots(true);
            }

            return true;
        }

        protected virtual void NetFuncOnDead()
        {
            CancelReload();
            CancelAttack();
            CancelSkill();
            ClearActionStates();
            if (onDead != null)
                onDead.Invoke();
        }

        protected virtual void NetFuncOnRespawn()
        {
            ClearActionStates();
            if (onRespawn != null)
                onRespawn.Invoke();
        }

        protected virtual void NetFuncOnLevelUp()
        {
            if (CurrentGameInstance.levelUpEffect != null)
                CharacterModel.InstantiateEffect(CurrentGameInstance.levelUpEffect);
            if (onLevelUp != null)
                onLevelUp.Invoke();
        }

        [ServerRpc]
        protected virtual void ServerUnSummon(uint objectId)
        {
#if !CLIENT_BUILD
            int index = this.IndexOfSummon(objectId);
            if (index < 0)
                return;

            CharacterSummon summon = Summons[index];
            if (summon.type != SummonType.Pet)
                return;

            Summons.RemoveAt(index);
            summon.UnSummon(this);
#endif
        }

        [ServerRpc]
        protected void ServerSwitchEquipWeaponSet(byte equipWeaponSet)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            if (equipWeaponSet >= CurrentGameInstance.maxEquipWeaponSet)
                equipWeaponSet = (byte)(CurrentGameInstance.maxEquipWeaponSet - 1);

            this.FillWeaponSetsIfNeeded(equipWeaponSet);
            EquipWeaponSet = equipWeaponSet;
#endif
        }
    }
}
