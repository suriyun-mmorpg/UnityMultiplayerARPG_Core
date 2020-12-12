namespace MultiplayerARPG
{
    public static class CharacterInventoryExtension
    {
        public static bool CanEquipWeapon(this ICharacterData character, CharacterItem equippingItem, byte equipWeaponSet, bool isLeftHand, out GameMessage.Type gameMessageType, out bool shouldUnequipRightHand, out bool shouldUnequipLeftHand)
        {
            shouldUnequipRightHand = false;
            shouldUnequipLeftHand = false;

            if (equippingItem.GetWeaponItem() == null && equippingItem.GetShieldItem() == null)
            {
                gameMessageType = GameMessage.Type.CannotEquip;
                return false;
            }

            if (!equippingItem.GetEquipmentItem().CanEquip(character, equippingItem.level, out gameMessageType))
                return false;

            character.FillWeaponSetsIfNeeded(equipWeaponSet);
            EquipWeapons tempEquipWeapons = character.SelectableWeaponSets[equipWeaponSet];

            WeaponItemEquipType rightHandEquipType;
            bool hasRightHandItem =
                tempEquipWeapons.GetRightHandWeaponItem().TryGetWeaponItemEquipType(out rightHandEquipType);
            WeaponItemEquipType leftHandEquipType;
            bool hasLeftHandItem =
                tempEquipWeapons.GetLeftHandWeaponItem().TryGetWeaponItemEquipType(out leftHandEquipType) ||
                tempEquipWeapons.GetLeftHandShieldItem() != null;

            // Equipping item is weapon
            IWeaponItem equippingWeaponItem = equippingItem.GetWeaponItem();
            if (equippingWeaponItem != null)
            {
                switch (equippingWeaponItem.EquipType)
                {
                    case WeaponItemEquipType.OneHand:
                        // If weapon is one hand its equip position must be right hand
                        if (isLeftHand)
                        {
                            gameMessageType = GameMessage.Type.InvalidEquipPositionRightHand;
                            return false;
                        }
                        // One hand can equip with shield only 
                        // if there are weapons on left hand it should unequip
                        if (hasRightHandItem)
                            shouldUnequipRightHand = true;
                        // Unequip left-hand weapon, don't unequip shield
                        if (hasLeftHandItem && tempEquipWeapons.GetLeftHandWeaponItem() != null)
                            shouldUnequipLeftHand = true;
                        break;
                    case WeaponItemEquipType.OneHandCanDual:
                        // If weapon is one hand can dual its equip position must be right or left hand
                        if (!isLeftHand && hasRightHandItem)
                        {
                            shouldUnequipRightHand = true;
                        }
                        if (isLeftHand && hasLeftHandItem)
                        {
                            shouldUnequipLeftHand = true;
                        }
                        // Unequip item if right hand weapon is one hand or two hand when equipping at left hand
                        if (isLeftHand && hasRightHandItem)
                        {
                            if (rightHandEquipType == WeaponItemEquipType.OneHand ||
                                rightHandEquipType == WeaponItemEquipType.TwoHand)
                                shouldUnequipRightHand = true;
                        }
                        break;
                    case WeaponItemEquipType.TwoHand:
                        // If weapon is one hand its equip position must be right hand
                        if (isLeftHand)
                        {
                            gameMessageType = GameMessage.Type.InvalidEquipPositionRightHand;
                            return false;
                        }
                        // Unequip both left and right hand
                        if (hasRightHandItem)
                            shouldUnequipRightHand = true;
                        if (hasLeftHandItem)
                            shouldUnequipLeftHand = true;
                        break;
                }
                return true;
            }

            // Equipping item is shield
            IShieldItem equippingShieldItem = equippingItem.GetShieldItem();
            if (equippingShieldItem != null)
            {
                // If it is shield, its equip position must be left hand
                if (!isLeftHand)
                {
                    gameMessageType = GameMessage.Type.InvalidEquipPositionLeftHand;
                    return false;
                }
                if (hasRightHandItem && rightHandEquipType == WeaponItemEquipType.TwoHand)
                    shouldUnequipRightHand = true;
                if (hasLeftHandItem)
                    shouldUnequipLeftHand = true;
                return true;
            }
            gameMessageType = GameMessage.Type.CannotEquip;
            return false;
        }

        public static bool CanEquipItem(this ICharacterData character, CharacterItem equippingItem, byte equipSlotIndex, out GameMessage.Type gameMessageType, out int unEquippingIndex)
        {
            unEquippingIndex = -1;

            if (equippingItem.GetArmorItem() == null)
            {
                gameMessageType = GameMessage.Type.CannotEquip;
                return false;
            }

            if (!equippingItem.GetEquipmentItem().CanEquip(character, equippingItem.level, out gameMessageType))
                return false;

            // Equipping item is armor
            IArmorItem equippingArmorItem = equippingItem.GetArmorItem();
            if (equippingArmorItem != null)
            {
                unEquippingIndex = character.IndexOfEquipItemByEquipPosition(equippingArmorItem.EquipPosition, equipSlotIndex);
                return true;
            }
            gameMessageType = GameMessage.Type.CannotEquip;
            return false;
        }

        public static bool EquipWeapon(this ICharacterData character, int nonEquipIndex, byte equipWeaponSet, bool isLeftHand, out GameMessage.Type gameMessageType)
        {
            if (character == null || nonEquipIndex < 0 ||nonEquipIndex >= character.NonEquipItems.Count)
            {
                gameMessageType = GameMessage.Type.InvalidItemData;
                return false;
            }

            CharacterItem equippingItem = character.NonEquipItems[nonEquipIndex];
            bool shouldUnequipRightHand;
            bool shouldUnequipLeftHand;
            if (!character.CanEquipWeapon(equippingItem, equipWeaponSet, isLeftHand, out gameMessageType, out shouldUnequipRightHand, out shouldUnequipLeftHand))
                return false;

            int unEquipCount = -1;
            if (shouldUnequipRightHand)
                ++unEquipCount;
            if (shouldUnequipLeftHand)
                ++unEquipCount;

            if (character.UnEquipItemWillOverwhelming(unEquipCount))
            {
                gameMessageType = GameMessage.Type.CannotCarryAnymore;
                return false;
            }

            int unEquippedIndexRightHand = -1;
            if (shouldUnequipRightHand)
            {
                if (!character.UnEquipWeapon(equipWeaponSet, false, true, out gameMessageType, out unEquippedIndexRightHand))
                    return false;
            }
            int unEquippedIndexLeftHand = -1;
            if (shouldUnequipLeftHand)
            {
                if (!character.UnEquipWeapon(equipWeaponSet, true, true, out gameMessageType, out unEquippedIndexLeftHand))
                    return false;
            }

            // Equipping items
            character.FillWeaponSetsIfNeeded(equipWeaponSet);
            EquipWeapons tempEquipWeapons = character.SelectableWeaponSets[equipWeaponSet];
            if (isLeftHand)
            {
                equippingItem.equipSlotIndex = equipWeaponSet;
                tempEquipWeapons.leftHand = equippingItem;
                character.SelectableWeaponSets[equipWeaponSet] = tempEquipWeapons;
            }
            else
            {
                equippingItem.equipSlotIndex = equipWeaponSet;
                tempEquipWeapons.rightHand = equippingItem;
                character.SelectableWeaponSets[equipWeaponSet] = tempEquipWeapons;
            }
            // Update inventory
            if (unEquippedIndexRightHand >= 0 && unEquippedIndexLeftHand >= 0)
            {
                // Swap with equipped item
                character.NonEquipItems[nonEquipIndex] = character.NonEquipItems[unEquippedIndexRightHand];
                if (GameInstance.Singleton.IsLimitInventorySlot)
                    character.NonEquipItems[unEquippedIndexRightHand] = CharacterItem.Empty;
                else
                    character.NonEquipItems.RemoveAt(unEquippedIndexRightHand);
                // Find empty slot for unequipped left-hand weapon to swap with empty slot
                if (GameInstance.Singleton.IsLimitInventorySlot)
                {
                    character.NonEquipItems[character.IndexOfEmptyNonEquipItemSlot()] = character.NonEquipItems[unEquippedIndexLeftHand];
                    character.NonEquipItems[unEquippedIndexLeftHand] = CharacterItem.Empty;
                }
            }
            else if (unEquippedIndexRightHand >= 0)
            {
                // Swap with equipped item
                character.NonEquipItems[nonEquipIndex] = character.NonEquipItems[unEquippedIndexRightHand];
                if (GameInstance.Singleton.IsLimitInventorySlot)
                    character.NonEquipItems[unEquippedIndexRightHand] = CharacterItem.Empty;
                else
                    character.NonEquipItems.RemoveAt(unEquippedIndexRightHand);
            }
            else if (unEquippedIndexLeftHand >= 0)
            {
                // Swap with equipped item
                character.NonEquipItems[nonEquipIndex] = character.NonEquipItems[unEquippedIndexLeftHand];
                if (GameInstance.Singleton.IsLimitInventorySlot)
                    character.NonEquipItems[unEquippedIndexLeftHand] = CharacterItem.Empty;
                else
                    character.NonEquipItems.RemoveAt(unEquippedIndexLeftHand);
            }
            else
            {
                // Remove equipped item
                if (GameInstance.Singleton.IsLimitInventorySlot)
                    character.NonEquipItems[nonEquipIndex] = CharacterItem.Empty;
                else
                    character.NonEquipItems.RemoveAt(nonEquipIndex);
            }
            character.FillEmptySlots(true);
            gameMessageType = GameMessage.Type.None;
            return true;
        }

        public static bool UnEquipWeapon(this ICharacterData character, byte equipWeaponSet, bool isLeftHand, bool doNotValidate, out GameMessage.Type gameMessageType, out int unEquippedIndex, int expectedUnequippedIndex = -1)
        {
            unEquippedIndex = -1;
            character.FillWeaponSetsIfNeeded(equipWeaponSet);
            EquipWeapons tempEquipWeapons = character.SelectableWeaponSets[equipWeaponSet];
            CharacterItem unEquipItem;

            if (isLeftHand)
            {
                // Unequip left-hand weapon
                unEquipItem = tempEquipWeapons.leftHand;
                if (!doNotValidate && unEquipItem.NotEmptySlot() &&
                    character.UnEquipItemWillOverwhelming())
                {
                    gameMessageType = GameMessage.Type.CannotCarryAnymore;
                    return false;
                }
                tempEquipWeapons.leftHand = CharacterItem.Empty;
                character.SelectableWeaponSets[equipWeaponSet] = tempEquipWeapons;
            }
            else
            {
                // Unequip right-hand weapon
                unEquipItem = tempEquipWeapons.rightHand;
                if (!doNotValidate && unEquipItem.NotEmptySlot() &&
                    character.UnEquipItemWillOverwhelming())
                {
                    gameMessageType = GameMessage.Type.CannotCarryAnymore;
                    return false;
                }
                tempEquipWeapons.rightHand = CharacterItem.Empty;
                character.SelectableWeaponSets[equipWeaponSet] = tempEquipWeapons;
            }

            if (unEquipItem.NotEmptySlot())
            {
                character.AddOrSetNonEquipItems(unEquipItem, out unEquippedIndex, expectedUnequippedIndex);
                character.FillEmptySlots(true);
            }
            gameMessageType = GameMessage.Type.None;
            return true;
        }


        public static bool EquipArmor(this ICharacterData character, int nonEquipIndex, byte equipSlotIndex, out GameMessage.Type gameMessageType)
        {
            if (character == null || nonEquipIndex < 0 || nonEquipIndex >= character.NonEquipItems.Count)
            {
                gameMessageType = GameMessage.Type.InvalidItemData;
                return false;
            }

            CharacterItem equippingItem = character.NonEquipItems[nonEquipIndex];
            int unEquippingIndex;
            if (!character.CanEquipItem(equippingItem, equipSlotIndex, out gameMessageType, out unEquippingIndex))
                return false;

            int unEquippedIndex = -1;
            if (unEquippingIndex >= 0 && !character.UnEquipArmor(unEquippingIndex, true, out gameMessageType, out unEquippedIndex))
                return false;

            // Can equip the item when there is no equipped item or able to unequip the equipped item
            equippingItem.equipSlotIndex = equipSlotIndex;
            character.EquipItems.Add(equippingItem);
            // Update inventory
            if (unEquippedIndex >= 0)
            {
                // Swap with equipped item
                character.NonEquipItems[nonEquipIndex] = character.NonEquipItems[unEquippedIndex];
                if (GameInstance.Singleton.IsLimitInventorySlot)
                    character.NonEquipItems[unEquippedIndex] = CharacterItem.Empty;
                else
                    character.NonEquipItems.RemoveAt(unEquippedIndex);
            }
            else
            {
                // Remove equipped item
                if (GameInstance.Singleton.IsLimitInventorySlot)
                    character.NonEquipItems[nonEquipIndex] = CharacterItem.Empty;
                else
                    character.NonEquipItems.RemoveAt(nonEquipIndex);
            }
            character.FillEmptySlots(true);
            gameMessageType = GameMessage.Type.None;
            return true;
        }

        public static bool UnEquipArmor(this ICharacterData character, int index, bool doNotValidate, out GameMessage.Type gameMessageType, out int unEquippedIndex, int expectedUnequippedIndex = -1)
        {
            unEquippedIndex = -1;
            if (character == null || index < 0 || index >= character.EquipItems.Count)
            {
                gameMessageType = GameMessage.Type.InvalidItemData;
                return false;
            }
            CharacterItem unEquipItem = character.EquipItems[index];
            if (!doNotValidate && unEquipItem.NotEmptySlot() &&
                character.UnEquipItemWillOverwhelming())
            {
                gameMessageType = GameMessage.Type.CannotCarryAnymore;
                return false;
            }
            character.EquipItems.RemoveAt(index);

            if (unEquipItem.NotEmptySlot())
            {
                character.AddOrSetNonEquipItems(unEquipItem, out unEquippedIndex, expectedUnequippedIndex);
                character.FillEmptySlots(true);
            }
            gameMessageType = GameMessage.Type.None;
            return true;
        }

        public static bool SwapOrMergeItem(this ICharacterData character, short fromIndex, short toIndex, out GameMessage.Type gameMessageType)
        {
            if (fromIndex < 0 || fromIndex >= character.NonEquipItems.Count ||
                toIndex < 0 || toIndex >= character.NonEquipItems.Count ||
                fromIndex == toIndex)
            {
                gameMessageType = GameMessage.Type.InvalidItemData;
                return false;
            }

            CharacterItem fromItem = character.NonEquipItems[fromIndex];
            CharacterItem toItem = character.NonEquipItems[toIndex];
            if (fromItem.dataId.Equals(toItem.dataId) && !fromItem.IsFull() && !toItem.IsFull())
            {
                // Merge if same id and not full
                short maxStack = toItem.GetMaxStack();
                if (toItem.amount + fromItem.amount <= maxStack)
                {
                    toItem.amount += fromItem.amount;
                    if (GameInstance.Singleton.IsLimitInventorySlot)
                        character.NonEquipItems[fromIndex] = CharacterItem.Empty;
                    else
                        character.NonEquipItems.RemoveAt(fromIndex);
                    character.NonEquipItems[toIndex] = toItem;
                    character.FillEmptySlots();
                }
                else
                {
                    short remains = (short)(toItem.amount + fromItem.amount - maxStack);
                    toItem.amount = maxStack;
                    fromItem.amount = remains;
                    character.NonEquipItems[fromIndex] = fromItem;
                    character.NonEquipItems[toIndex] = toItem;
                }
            }
            else
            {
                // Swap
                character.NonEquipItems[fromIndex] = toItem;
                character.NonEquipItems[toIndex] = fromItem;
            }
            gameMessageType = GameMessage.Type.None;
            return true;
        }
    }
}
