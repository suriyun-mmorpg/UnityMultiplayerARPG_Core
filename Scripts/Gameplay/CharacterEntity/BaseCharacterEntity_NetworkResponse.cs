using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public System.Action onDead;
        public System.Action onRespawn;
        public System.Action onLevelUp;

        protected void NetFuncPlayAttack(bool isLeftHand, byte animationIndex, Vector3 aimPosition)
        {
            StartCoroutine(AttackRoutine(
                isLeftHand,
                animationIndex,
                aimPosition));
        }

        protected void NetFuncPlayUseSkill(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel, Vector3 aimPosition)
        {
            BaseSkill skill;
            if (GameInstance.Skills.TryGetValue(skillDataId, out skill) && skillLevel > 0)
            {
                StartCoroutine(UseSkillRoutine(
                    isLeftHand,
                    animationIndex,
                    skill,
                    skillLevel,
                    aimPosition));
            }
            else
            {
                animActionType = AnimActionType.None;
                IsAttackingOrUsingSkill = false;
            }
        }

        protected void NetFuncPlayReload(bool isLeftHand)
        {
            StartCoroutine(ReloadRoutine(isLeftHand));
        }

        /// <summary>
        /// This will be called at server to order character to pickup items
        /// </summary>
        /// <param name="objectId"></param>
        protected virtual void NetFuncPickupItem(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            ItemDropEntity itemDropEntity = null;
            if (!this.TryGetEntityByObjectId(objectId, out itemDropEntity))
                return;

            if (Vector3.Distance(CacheTransform.position, itemDropEntity.CacheTransform.position) > gameInstance.pickUpItemDistance + 5f)
                return;

            if (!itemDropEntity.IsAbleToLoot(this))
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.NotAbleToLoot);
                return;
            }

            CharacterItem itemDropData = itemDropEntity.dropData;
            if (itemDropData.IsEmptySlot())
            {
                // Destroy item drop entity without item add because this is not valid
                itemDropEntity.MarkAsPickedUp();
                itemDropEntity.NetworkDestroy();
                return;
            }
            if (!this.IncreasingItemsWillOverwhelming(itemDropData.dataId, itemDropData.amount) && this.IncreaseItems(itemDropData))
            {
                itemDropEntity.MarkAsPickedUp();
                itemDropEntity.NetworkDestroy();
            }
        }

        /// <summary>
        /// This will be called at server to order character to drop items
        /// </summary>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        protected virtual void NetFuncDropItem(short index, short amount)
        {
            if (!CanDoActions() ||
                index >= nonEquipItems.Count)
                return;

            CharacterItem nonEquipItem = nonEquipItems[index];
            if (nonEquipItem.IsEmptySlot() || amount > nonEquipItem.amount)
                return;

            if (this.DecreaseItemsByIndex(index, amount))
            {
                // Drop item to the ground
                CharacterItem dropData = nonEquipItem.Clone();
                dropData.amount = amount;
                ItemDropEntity.DropItem(this, dropData, new uint[] { ObjectId });
            }
        }

        /// <summary>
        /// This will be called at server to order character to equip weapon or shield
        /// </summary>
        /// <param name="nonEquipIndex"></param>
        /// <param name="equipWeaponSet"></param>
        /// <param name="isLeftHand"></param>
        protected virtual void NetFuncEquipWeapon(short nonEquipIndex, byte equipWeaponSet, bool isLeftHand)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= nonEquipItems.Count)
                return;

            CharacterItem equippingItem = nonEquipItems[nonEquipIndex];

            GameMessage.Type gameMessageType;
            bool shouldUnequipRightHand;
            bool shouldUnequipLeftHand;
            if (!CanEquipWeapon(equippingItem, equipWeaponSet, isLeftHand, out gameMessageType, out shouldUnequipRightHand, out shouldUnequipLeftHand))
            {
                gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                return;
            }

            int unEquipCount = -1;
            if (shouldUnequipRightHand)
                ++unEquipCount;
            if (shouldUnequipLeftHand)
                ++unEquipCount;

            if (this.UnEquipItemWillOverwhelming(unEquipCount))
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotCarryAnymore);
                return;
            }

            if (shouldUnequipRightHand)
            {
                if (!UnEquipWeapon(equipWeaponSet, false, true, out gameMessageType))
                {
                    gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    return;
                }
            }
            if (shouldUnequipLeftHand)
            {
                if (!UnEquipWeapon(equipWeaponSet, true, true, out gameMessageType))
                {
                    gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
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
            nonEquipItems.RemoveAt(nonEquipIndex);
            this.FillEmptySlots();
        }

        /// <summary>
        /// This will be called at server to order character to equip armor
        /// </summary>
        /// <param name="nonEquipIndex"></param>
        /// <param name="equipSlotIndex"></param>
        protected virtual void NetFuncEquipArmor(short nonEquipIndex, byte equipSlotIndex)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= nonEquipItems.Count)
                return;

            CharacterItem equippingItem = nonEquipItems[nonEquipIndex];

            short unEquippingIndex = -1;
            GameMessage.Type gameMessageType;
            if (!CanEquipItem(equippingItem, equipSlotIndex, out gameMessageType, out unEquippingIndex))
            {
                gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                return;
            }

            if (unEquippingIndex >= 0 && !UnEquipArmor(unEquippingIndex, true, out gameMessageType))
            {
                gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                return;
            }

            // Can equip the item when there is no equipped item or able to unequip the equipped item
            equippingItem.equipSlotIndex = equipSlotIndex;
            equipItems.Add(equippingItem);
            // Update equip item indexes
            equipItemIndexes[GetEquipPosition(equippingItem.GetArmorItem().EquipPosition, equipSlotIndex)] = equipItems.Count - 1;
            // Update inventory
            nonEquipItems.RemoveAt(nonEquipIndex);
            this.FillEmptySlots();
        }

        protected virtual void NetFuncUnEquipWeapon(byte equipWeaponSet, bool isLeftHand)
        {
            GameMessage.Type gameMessageType;
            if (!UnEquipWeapon(equipWeaponSet, isLeftHand, false, out gameMessageType))
            {
                // Cannot unequip weapon, send reasons to client
                gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
            }
        }

        protected bool UnEquipWeapon(byte equipWeaponSet, bool isLeftHand, bool doNotValidate, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.None;
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
                this.AddOrSetNonEquipItems(unEquipItem);
                this.FillEmptySlots();
            }

            return true;
        }

        /// <summary>
        /// This will be called at server to order character to unequip equipments
        /// </summary>
        /// <param name="index"></param>
        protected virtual void NetFuncUnEquipArmor(short index)
        {
            GameMessage.Type gameMessageType;
            if (!UnEquipArmor(index, false, out gameMessageType))
            {
                // Cannot unequip weapon, send reasons to client
                gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
            }
        }

        protected bool UnEquipArmor(short index, bool doNotValidate, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.None;
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
            UpdateEquipItemIndexes();

            if (unEquipItem.NotEmptySlot())
            {
                this.AddOrSetNonEquipItems(unEquipItem);
                this.FillEmptySlots();
            }

            return true;
        }

        protected virtual void NetFuncOnDead()
        {
            animActionType = AnimActionType.None;
            if (onDead != null)
                onDead.Invoke();
        }

        protected virtual void NetFuncOnRespawn()
        {
            animActionType = AnimActionType.None;
            if (onRespawn != null)
                onRespawn.Invoke();
        }

        protected virtual void NetFuncOnLevelUp()
        {
            if (gameInstance.levelUpEffect != null)
                CharacterModel.InstantiateEffect(new GameEffect[] { gameInstance.levelUpEffect });
            if (onLevelUp != null)
                onLevelUp.Invoke();
        }

        protected virtual void NetFuncUnSummon(PackedUInt objectId)
        {
            int index = this.IndexOfSummon(objectId);
            if (index < 0)
                return;

            CharacterSummon summon = Summons[index];
            if (summon.type != SummonType.Pet)
                return;

            Summons.RemoveAt(index);
            summon.UnSummon(this);
        }

        protected void NetFuncMergeNonEquipItems(short index1, short index2)
        {
            if (!CanDoActions() ||
                index1 >= nonEquipItems.Count ||
                index2 >= nonEquipItems.Count)
                return;

            CharacterItem nonEquipItem1 = nonEquipItems[index1];
            CharacterItem nonEquipItem2 = nonEquipItems[index2];
            short maxStack = nonEquipItem2.GetMaxStack();
            if (nonEquipItem2.amount + nonEquipItem1.amount <= maxStack)
            {
                nonEquipItem2.amount += nonEquipItem1.amount;
                nonEquipItems[index2] = nonEquipItem2;
                nonEquipItems.RemoveAt(index1);
            }
            else
            {
                short mergeAmount = (short)(maxStack - nonEquipItem2.amount);
                nonEquipItem2.amount = maxStack;
                nonEquipItem1.amount -= mergeAmount;
                nonEquipItems[index1] = nonEquipItem1;
                nonEquipItems[index2] = nonEquipItem2;
            }
        }

        protected void NetFuncSwapNonEquipItems(short index1, short index2)
        {
            if (!CanDoActions() ||
                index1 >= nonEquipItems.Count ||
                index2 >= nonEquipItems.Count)
                return;

            CharacterItem nonEquipItem1 = nonEquipItems[index1];
            CharacterItem nonEquipItem2 = nonEquipItems[index2];

            nonEquipItems[index2] = nonEquipItem1;
            nonEquipItems[index1] = nonEquipItem2;
        }

        protected void NetFuncSwitchEquipWeaponSet(byte equipWeaponSet)
        {
            if (!CanDoActions())
                return;

            if (equipWeaponSet >= gameInstance.maxEquipWeaponSet)
                equipWeaponSet = (byte)(gameInstance.maxEquipWeaponSet - 1);

            this.FillWeaponSetsIfNeeded(equipWeaponSet);
            EquipWeaponSet = equipWeaponSet;
        }
    }
}
