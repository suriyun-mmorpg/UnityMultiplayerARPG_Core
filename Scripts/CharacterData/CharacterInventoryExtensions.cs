using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class CharacterInventoryExtensions
    {
        public static bool MoveItemFromStorage(
            this IPlayerCharacterData playerCharacter,
            bool storageIsLimitSlot,
            int storageSlotLimit,
            IList<CharacterItem> storageItems,
            int storageItemIndex,
            int storageItemAmount,
            InventoryType inventoryType,
            int inventoryItemIndex,
            byte equipSlotIndexOrWeaponSet,
            out UITextKeys gameMessage)
        {
            // Prepare item data
            switch (inventoryType)
            {
                case InventoryType.EquipWeaponLeft:
                case InventoryType.EquipWeaponRight:
                case InventoryType.EquipItems:
                    if (!playerCharacter.SwapStorageItemWithEquipmentItem(storageIsLimitSlot, storageSlotLimit, storageItems, storageItemIndex, inventoryType, equipSlotIndexOrWeaponSet, out gameMessage))
                        return false;
                    break;
                default:
                    if (storageItems[storageItemIndex].IsEmptySlot())
                    {
                        gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                        return false;
                    }
                    if (storageItems[storageItemIndex].amount < storageItemAmount)
                    {
                        gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS;
                        return false;
                    }
                    CharacterItem movingItem = storageItems[storageItemIndex].Clone(true);
                    movingItem.amount = storageItemAmount;
                    if (inventoryItemIndex < 0 || inventoryItemIndex >= playerCharacter.NonEquipItems.Count ||
                        playerCharacter.NonEquipItems[inventoryItemIndex].dataId == movingItem.dataId)
                    {
                        // Add to inventory or merge
                        bool isOverwhelming = playerCharacter.IncreasingItemsWillOverwhelming(movingItem.dataId, movingItem.amount);
                        if (isOverwhelming || !playerCharacter.IncreaseItems(movingItem))
                        {
                            gameMessage = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                            return false;
                        }
                        // Remove from storage
                        storageItems.DecreaseItemsByIndex(storageItemIndex, storageItemAmount, storageIsLimitSlot, true);
                    }
                    else
                    {
                        // Already check for the storage index, so don't do it again
                        if (playerCharacter.NonEquipItems[inventoryItemIndex].IsEmptySlot())
                        {
                            // Replace empty slot
                            playerCharacter.NonEquipItems[inventoryItemIndex] = movingItem;
                            // Remove from storage
                            storageItems.DecreaseItemsByIndex(storageItemIndex, storageItemAmount, storageIsLimitSlot, true);
                        }
                        else
                        {
                            // Swapping
                            CharacterItem storageItem = storageItems[storageItemIndex].Clone(true);
                            CharacterItem nonEquipItem = playerCharacter.NonEquipItems[inventoryItemIndex].Clone(true);
                            storageItems[storageItemIndex] = nonEquipItem;
                            playerCharacter.NonEquipItems[inventoryItemIndex] = storageItem;
                        }
                    }
                    break;
            }
            storageItems.FillEmptySlots(storageIsLimitSlot, storageSlotLimit);
            playerCharacter.FillEmptySlots();
            gameMessage = UITextKeys.NONE;
            return true;
        }

        public static bool MoveItemToStorage(
            this IPlayerCharacterData playerCharacter,
            bool storageIsLimitWeight,
            float storageWeightLimit,
            bool storageIsLimitSlot,
            int storageSlotLimit,
            IList<CharacterItem> storageItems,
            int storageItemIndex,
            InventoryType inventoryType,
            int inventoryItemIndex,
            int inventoryItemAmount,
            byte equipSlotIndexOrWeaponSet,
            out UITextKeys gameMessage)
        {
            // Get and validate inventory item
            if (equipSlotIndexOrWeaponSet < 0 || equipSlotIndexOrWeaponSet >= GameInstance.Singleton.maxEquipWeaponSet)
                equipSlotIndexOrWeaponSet = playerCharacter.EquipWeaponSet;
            playerCharacter.FillWeaponSetsIfNeeded(equipSlotIndexOrWeaponSet);
            EquipWeapons equipWeapons = playerCharacter.SelectableWeaponSets[equipSlotIndexOrWeaponSet];
            CharacterItem movingItem;

            // Validate item index
            switch (inventoryType)
            {
                case InventoryType.EquipWeaponLeft:
                    if (equipWeapons.leftHand.IsEmptySlot())
                    {
                        gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                        return false;
                    }
                    movingItem = equipWeapons.leftHand.Clone(true);
                    break;
                case InventoryType.EquipWeaponRight:
                    if (equipWeapons.rightHand.IsEmptySlot())
                    {
                        gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                        return false;
                    }
                    movingItem = equipWeapons.rightHand.Clone(true);
                    break;
                case InventoryType.EquipItems:
                    if (inventoryItemIndex < 0 || inventoryItemIndex >= playerCharacter.EquipItems.Count)
                    {
                        gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                        return false;
                    }
                    if (playerCharacter.EquipItems[inventoryItemIndex].IsEmptySlot())
                    {
                        gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                        return false;
                    }
                    movingItem = playerCharacter.EquipItems[inventoryItemIndex].Clone(true);
                    break;
                default:
                    if (inventoryItemIndex < 0 || inventoryItemIndex >= playerCharacter.NonEquipItems.Count)
                    {
                        gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                        return false;
                    }
                    if (playerCharacter.NonEquipItems[inventoryItemIndex].IsEmptySlot())
                    {
                        gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                        return false;
                    }
                    if (playerCharacter.NonEquipItems[inventoryItemIndex].amount < inventoryItemAmount)
                    {
                        gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS;
                        return false;
                    }
                    movingItem = playerCharacter.NonEquipItems[inventoryItemIndex].Clone(true);
                    movingItem.amount = inventoryItemAmount;
                    break;
            }

            switch (inventoryType)
            {
                case InventoryType.EquipWeaponLeft:
                case InventoryType.EquipWeaponRight:
                case InventoryType.EquipItems:
                    if (storageItemIndex < 0 || storageItemIndex >= storageItems.Count ||
                        storageItems[storageItemIndex].IsEmptySlot())
                    {
                        // Add to storage
                        bool isOverwhelming = storageItems.IncreasingItemsWillOverwhelming(
                            movingItem.dataId, movingItem.amount, storageIsLimitWeight, storageWeightLimit,
                            storageItems.GetTotalItemWeight(), storageIsLimitSlot, storageSlotLimit);
                        if (isOverwhelming || !storageItems.IncreaseItems(movingItem))
                        {
                            gameMessage = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                            return false;
                        }
                        // Remove from inventory
                        switch (inventoryType)
                        {
                            case InventoryType.EquipWeaponLeft:
                                equipWeapons.leftHand = CharacterItem.Empty;
                                playerCharacter.SelectableWeaponSets[equipSlotIndexOrWeaponSet] = equipWeapons;
                                break;
                            case InventoryType.EquipWeaponRight:
                                equipWeapons.rightHand = CharacterItem.Empty;
                                playerCharacter.SelectableWeaponSets[equipSlotIndexOrWeaponSet] = equipWeapons;
                                break;
                            case InventoryType.EquipItems:
                                playerCharacter.EquipItems.RemoveAt(inventoryItemIndex);
                                break;
                        }
                    }
                    else
                    {
                        // Swapping
                        if (!playerCharacter.SwapStorageItemWithEquipmentItem(storageIsLimitSlot, storageSlotLimit, storageItems, storageItemIndex, inventoryType, equipSlotIndexOrWeaponSet, out gameMessage))
                            return false;
                    }
                    break;
                default:
                    if (storageItemIndex < 0 || storageItemIndex >= storageItems.Count ||
                        storageItems[storageItemIndex].dataId == movingItem.dataId)
                    {
                        // Add to storage or merge
                        bool isOverwhelming = storageItems.IncreasingItemsWillOverwhelming(
                            movingItem.dataId, movingItem.amount, storageIsLimitWeight, storageWeightLimit,
                            storageItems.GetTotalItemWeight(), storageIsLimitSlot, storageSlotLimit);
                        if (isOverwhelming || !storageItems.IncreaseItems(movingItem))
                        {
                            gameMessage = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                            return false;
                        }
                        // Remove from inventory
                        playerCharacter.DecreaseItemsByIndex(inventoryItemIndex, inventoryItemAmount, true);
                    }
                    else
                    {
                        // Already check for the storage index, so don't do it again
                        if (storageItems[storageItemIndex].IsEmptySlot())
                        {
                            // Replace empty slot
                            storageItems[storageItemIndex] = movingItem;
                            // Remove from inventory
                            playerCharacter.DecreaseItemsByIndex(inventoryItemIndex, inventoryItemAmount, true);
                        }
                        else
                        {
                            // Swapping
                            CharacterItem storageItem = storageItems[storageItemIndex].Clone(true);
                            CharacterItem nonEquipItem = playerCharacter.NonEquipItems[inventoryItemIndex].Clone(true);
                            storageItems[storageItemIndex] = nonEquipItem;
                            playerCharacter.NonEquipItems[inventoryItemIndex] = storageItem;
                        }
                    }
                    break;
            }
            storageItems.FillEmptySlots(storageIsLimitSlot, storageSlotLimit);
            playerCharacter.FillEmptySlots();
            gameMessage = UITextKeys.NONE;
            return true;
        }

        public static bool SwapStorageItemWithEquipmentItem(
            this IPlayerCharacterData playerCharacter,
            bool storageIsLimitSlot,
            int storageSlotLimit,
            IList<CharacterItem> storageItems,
            int storageItemIndex,
            InventoryType inventoryType,
            byte equipSlotIndexOrWeaponSet,
            out UITextKeys gameMessage)
        {
            if (storageItemIndex >= storageItems.Count)
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                return false;
            }
            CharacterItem movingItem = storageItems[storageItemIndex].Clone(true);
            if (movingItem.IsEmptySlot())
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                return false;
            }
            // Prepare variables that being used in switch scope
            if (equipSlotIndexOrWeaponSet < 0 || equipSlotIndexOrWeaponSet >= GameInstance.Singleton.maxEquipWeaponSet)
                equipSlotIndexOrWeaponSet = playerCharacter.EquipWeaponSet;
            playerCharacter.FillWeaponSetsIfNeeded(equipSlotIndexOrWeaponSet);
            EquipWeapons equipWeapons = playerCharacter.SelectableWeaponSets[equipSlotIndexOrWeaponSet];
            bool shouldUnequipRightHand;
            bool shouldUnequipLeftHand;
            int unequippingIndex;
            switch (inventoryType)
            {
                case InventoryType.EquipWeaponLeft:
                    if (!playerCharacter.CanEquipWeapon(movingItem, equipSlotIndexOrWeaponSet, true, out gameMessage, out shouldUnequipRightHand, out shouldUnequipLeftHand))
                        return false;
                    // Validate unequipping right-hand item only, for the left one it will be swapped with storage item
                    if (shouldUnequipRightHand)
                    {
                        if (!playerCharacter.UnEquipWeapon(equipSlotIndexOrWeaponSet, false, false, out gameMessage, out _))
                            return false;
                    }
                    // Just equip or swapping
                    if (equipWeapons.IsEmptyLeftHandSlot())
                    {
                        // Just equip
                        equipWeapons.leftHand = movingItem;
                        playerCharacter.SelectableWeaponSets[equipSlotIndexOrWeaponSet] = equipWeapons;
                        // Remove from storage
                        storageItems.DecreaseItemsByIndex(storageItemIndex, movingItem.amount, storageIsLimitSlot, true);
                    }
                    else
                    {
                        // Swapping
                        CharacterItem storageItem = storageItems[storageItemIndex].Clone(true);
                        CharacterItem equipmentItem = equipWeapons.leftHand.Clone(true);
                        storageItems[storageItemIndex] = equipmentItem;
                        equipWeapons.leftHand = storageItem;
                        playerCharacter.SelectableWeaponSets[equipSlotIndexOrWeaponSet] = equipWeapons;
                    }
                    return true;
                case InventoryType.EquipWeaponRight:
                    if (!playerCharacter.CanEquipWeapon(movingItem, equipSlotIndexOrWeaponSet, false, out gameMessage, out shouldUnequipRightHand, out shouldUnequipLeftHand))
                        return false;
                    // Validate unequipping left-hand item only, for the right one it will be swapped with storage item
                    if (shouldUnequipLeftHand)
                    {
                        if (!playerCharacter.UnEquipWeapon(equipSlotIndexOrWeaponSet, true, false, out gameMessage, out _))
                            return false;
                    }
                    // Just equip or swapping
                    if (equipWeapons.IsEmptyRightHandSlot())
                    {
                        // Just equip
                        equipWeapons.rightHand = movingItem;
                        playerCharacter.SelectableWeaponSets[equipSlotIndexOrWeaponSet] = equipWeapons;
                        // Remove from storage
                        storageItems.DecreaseItemsByIndex(storageItemIndex, movingItem.amount, storageIsLimitSlot, true);
                    }
                    else
                    {
                        // Swapping
                        CharacterItem storageItem = storageItems[storageItemIndex].Clone(true);
                        CharacterItem equipmentItem = equipWeapons.rightHand.Clone(true);
                        storageItems[storageItemIndex] = equipmentItem;
                        equipWeapons.rightHand = storageItem;
                        playerCharacter.SelectableWeaponSets[equipSlotIndexOrWeaponSet] = equipWeapons;
                    }
                    return true;
                case InventoryType.EquipItems:
                    if (!playerCharacter.CanEquipItem(movingItem, equipSlotIndexOrWeaponSet, out gameMessage, out unequippingIndex))
                        return false;
                    // Just equip or swapping
                    if (unequippingIndex < 0)
                    {
                        // Just equip
                        movingItem.equipSlotIndex = equipSlotIndexOrWeaponSet;
                        playerCharacter.EquipItems.Add(movingItem);
                        // Remove from storage
                        storageItems.DecreaseItemsByIndex(storageItemIndex, movingItem.amount, storageIsLimitSlot, true);
                    }
                    else
                    {
                        // Swapping
                        CharacterItem storageItem = storageItems[storageItemIndex].Clone(true);
                        CharacterItem equipItem = playerCharacter.EquipItems[unequippingIndex].Clone(true);
                        storageItem.equipSlotIndex = equipSlotIndexOrWeaponSet;
                        equipItem.equipSlotIndex = 0;
                        storageItems[storageItemIndex] = equipItem;
                        playerCharacter.EquipItems[unequippingIndex] = storageItem;
                    }
                    return true;
                default:
                    gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_DATA;
                    return false;
            }
        }

        public static bool CanEquipWeapon(this ICharacterData character, CharacterItem equippingItem, byte equipWeaponSet, bool isLeftHand, out UITextKeys gameMessage, out bool shouldUnequipRightHand, out bool shouldUnequipLeftHand)
        {
            shouldUnequipRightHand = false;
            shouldUnequipLeftHand = false;

            if (equippingItem.GetWeaponItem() == null && equippingItem.GetShieldItem() == null)
            {
                gameMessage = UITextKeys.UI_ERROR_CANNOT_EQUIP;
                return false;
            }

            if (!equippingItem.GetEquipmentItem().CanEquip(character, equippingItem.level, out gameMessage))
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
                switch (equippingWeaponItem.GetEquipType())
                {
                    case WeaponItemEquipType.MainHandOnly:
                        // If weapon is main-hand only its equip position must be right hand
                        if (isLeftHand)
                        {
                            gameMessage = UITextKeys.UI_ERROR_INVALID_EQUIP_POSITION_RIGHT_HAND;
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
                    case WeaponItemEquipType.DualWieldable:
                        DualWieldRestriction dualWieldRestriction = equippingWeaponItem.GetDualWieldRestriction();
                        if (dualWieldRestriction == DualWieldRestriction.MainHandRestricted && !isLeftHand)
                        {
                            gameMessage = UITextKeys.UI_ERROR_INVALID_EQUIP_POSITION_LEFT_HAND;
                            return false;
                        }
                        if (dualWieldRestriction == DualWieldRestriction.OffHandRestricted && isLeftHand)
                        {
                            gameMessage = UITextKeys.UI_ERROR_INVALID_EQUIP_POSITION_RIGHT_HAND;
                            return false;
                        }
                        // If weapon is one hand can dual its equip position must be right or left hand
                        if (!isLeftHand && hasRightHandItem)
                        {
                            shouldUnequipRightHand = true;
                        }
                        if (isLeftHand && hasLeftHandItem)
                        {
                            shouldUnequipLeftHand = true;
                        }
                        // Unequip item if right hand weapon is main-hand only or two-hand when equipping at left-hand
                        if (isLeftHand && hasRightHandItem)
                        {
                            if (rightHandEquipType == WeaponItemEquipType.MainHandOnly ||
                                rightHandEquipType == WeaponItemEquipType.TwoHand)
                                shouldUnequipRightHand = true;
                        }
                        // Unequip item if left hand weapon is off-hand only when equipping at right-hand
                        if (!isLeftHand && hasLeftHandItem)
                        {
                            if (leftHandEquipType == WeaponItemEquipType.OffHandOnly)
                                shouldUnequipLeftHand = true;
                        }
                        break;
                    case WeaponItemEquipType.TwoHand:
                        // If weapon is one hand its equip position must be right hand
                        if (isLeftHand)
                        {
                            gameMessage = UITextKeys.UI_ERROR_INVALID_EQUIP_POSITION_RIGHT_HAND;
                            return false;
                        }
                        // Unequip both left and right hand
                        if (hasRightHandItem)
                            shouldUnequipRightHand = true;
                        if (hasLeftHandItem)
                            shouldUnequipLeftHand = true;
                        break;
                    case WeaponItemEquipType.OffHandOnly:
                        // If weapon is off-hand only its equip position must be left hand
                        if (!isLeftHand)
                        {
                            gameMessage = UITextKeys.UI_ERROR_INVALID_EQUIP_POSITION_LEFT_HAND;
                            return false;
                        }
                        // Unequip both left and right hand (there is no shield for main-hand)
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
                    gameMessage = UITextKeys.UI_ERROR_INVALID_EQUIP_POSITION_LEFT_HAND;
                    return false;
                }
                if (hasRightHandItem && rightHandEquipType == WeaponItemEquipType.TwoHand)
                    shouldUnequipRightHand = true;
                if (hasLeftHandItem)
                    shouldUnequipLeftHand = true;
                return true;
            }
            gameMessage = UITextKeys.UI_ERROR_CANNOT_EQUIP;
            return false;
        }

        public static bool CanEquipItem(this ICharacterData character, CharacterItem equippingItem, byte equipSlotIndex, out UITextKeys gameMessage, out int unEquippingIndex)
        {
            unEquippingIndex = -1;

            if (equippingItem.GetArmorItem() == null)
            {
                gameMessage = UITextKeys.UI_ERROR_CANNOT_EQUIP;
                return false;
            }

            if (!equippingItem.GetEquipmentItem().CanEquip(character, equippingItem.level, out gameMessage))
                return false;

            // Equipping item is armor
            IArmorItem equippingArmorItem = equippingItem.GetArmorItem();
            if (equippingArmorItem != null)
            {
                unEquippingIndex = character.IndexOfEquipItemByEquipPosition(equippingArmorItem.GetEquipPosition(), equipSlotIndex);
                return true;
            }
            gameMessage = UITextKeys.UI_ERROR_CANNOT_EQUIP;
            return false;
        }

        public static bool EquipWeapon(this ICharacterData character, int nonEquipIndex, byte equipWeaponSet, bool isLeftHand, out UITextKeys gameMessage)
        {
            if (nonEquipIndex < 0 || nonEquipIndex >= character.NonEquipItems.Count)
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                return false;
            }

            CharacterItem equippingItem = character.NonEquipItems[nonEquipIndex];
            bool shouldUnequipRightHand;
            bool shouldUnequipLeftHand;
            if (!character.CanEquipWeapon(equippingItem, equipWeaponSet, isLeftHand, out gameMessage, out shouldUnequipRightHand, out shouldUnequipLeftHand))
                return false;

            int unEquipCount = -1;
            if (shouldUnequipRightHand)
                ++unEquipCount;
            if (shouldUnequipLeftHand)
                ++unEquipCount;

            if (character.UnEquipItemWillOverwhelming(unEquipCount))
            {
                gameMessage = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                return false;
            }

            int unEquippedIndexRightHand = -1;
            if (shouldUnequipRightHand)
            {
                if (!character.UnEquipWeapon(equipWeaponSet, false, true, out gameMessage, out unEquippedIndexRightHand, fillEmptySlots: false))
                    return false;
            }
            int unEquippedIndexLeftHand = -1;
            if (shouldUnequipLeftHand)
            {
                if (!character.UnEquipWeapon(equipWeaponSet, true, true, out gameMessage, out unEquippedIndexLeftHand, fillEmptySlots: false))
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
            gameMessage = UITextKeys.NONE;
            return true;
        }

        public static bool UnEquipWeapon(this ICharacterData character, byte equipWeaponSet, bool isLeftHand, bool doNotValidate, out UITextKeys gameMessage, out int unEquippedIndex, int expectedUnequippedIndex = -1, bool fillEmptySlots = true)
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
                    gameMessage = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
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
                    gameMessage = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                    return false;
                }
                tempEquipWeapons.rightHand = CharacterItem.Empty;
                character.SelectableWeaponSets[equipWeaponSet] = tempEquipWeapons;
            }

            if (unEquipItem.NotEmptySlot())
            {
                character.AddOrSetNonEquipItems(unEquipItem, out unEquippedIndex, expectedUnequippedIndex);
                if (fillEmptySlots)
                    character.FillEmptySlots(true);
            }
            gameMessage = UITextKeys.NONE;
            return true;
        }

        public static bool EquipArmor(this ICharacterData character, int nonEquipIndex, byte equipSlotIndex, out UITextKeys gameMessage)
        {
            if (nonEquipIndex < 0 || nonEquipIndex >= character.NonEquipItems.Count)
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                return false;
            }

            CharacterItem equippingItem = character.NonEquipItems[nonEquipIndex];
            int unEquippingIndex;
            if (!character.CanEquipItem(equippingItem, equipSlotIndex, out gameMessage, out unEquippingIndex))
                return false;

            int unEquippedIndex = -1;
            if (unEquippingIndex >= 0 && !character.UnEquipArmor(unEquippingIndex, true, out gameMessage, out unEquippedIndex, fillEmptySlots: false))
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
            gameMessage = UITextKeys.NONE;
            return true;
        }

        public static bool UnEquipArmor(this ICharacterData character, int index, bool doNotValidate, out UITextKeys gameMessage, out int unEquippedIndex, int expectedUnequippedIndex = -1, bool fillEmptySlots = true)
        {
            unEquippedIndex = -1;
            if (index < 0 || index >= character.EquipItems.Count)
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                return false;
            }
            CharacterItem unEquipItem = character.EquipItems[index];
            if (!doNotValidate && unEquipItem.NotEmptySlot() &&
                character.UnEquipItemWillOverwhelming())
            {
                gameMessage = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                return false;
            }
            character.EquipItems.RemoveAt(index);

            if (unEquipItem.NotEmptySlot())
            {
                character.AddOrSetNonEquipItems(unEquipItem, out unEquippedIndex, expectedUnequippedIndex);
                if (fillEmptySlots)
                    character.FillEmptySlots(true);
            }
            gameMessage = UITextKeys.NONE;
            return true;
        }

        public static bool SwapOrMergeItem(this ICharacterData character, int fromIndex, int toIndex, out UITextKeys gameMessage)
        {
            if (fromIndex < 0 || fromIndex >= character.NonEquipItems.Count ||
                toIndex < 0 || toIndex >= character.NonEquipItems.Count ||
                fromIndex == toIndex)
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                return false;
            }

            CharacterItem fromItem = character.NonEquipItems[fromIndex];
            CharacterItem toItem = character.NonEquipItems[toIndex];
            if (fromItem.dataId.Equals(toItem.dataId) && !fromItem.IsFull() && !toItem.IsFull())
            {
                // Merge if same id and not full
                int maxStack = toItem.GetMaxStack();
                if (toItem.amount + fromItem.amount <= maxStack)
                {
                    toItem.amount += fromItem.amount;
                    character.NonEquipItems[toIndex] = toItem;
                    if (GameInstance.Singleton.IsLimitInventorySlot)
                        character.NonEquipItems[fromIndex] = CharacterItem.Empty;
                    else
                        character.NonEquipItems.RemoveAt(fromIndex);
                    character.FillEmptySlots();
                }
                else
                {
                    int remains = toItem.amount + fromItem.amount - maxStack;
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
            gameMessage = UITextKeys.NONE;
            return true;
        }

        public static bool VerifyDismantleItem(this IPlayerCharacterData character, int index, int amount, List<CharacterItem> simulatingNonEquipItems, out UITextKeys gameMessage, out int returningGold, out List<ItemAmount> returningItems, out List<CurrencyAmount> returningCurrencies)
        {
            gameMessage = UITextKeys.NONE;
            returningGold = 0;
            returningItems = null;
            returningCurrencies = null;

            if (index < 0 || index >= character.NonEquipItems.Count)
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                return false;
            }

            // Found item or not?
            CharacterItem nonEquipItem = character.NonEquipItems[index];
            if (nonEquipItem.IsEmptySlot() || amount > nonEquipItem.amount)
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS;
                return false;
            }

            if (!GameInstance.Singleton.dismantleFilter.Filter(nonEquipItem))
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_DATA;
                return false;
            }

            // Simulate data before applies
            if (!simulatingNonEquipItems.DecreaseItemsByIndex(index, amount, GameInstance.Singleton.IsLimitInventorySlot, false))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS;
                return false;
            }

            // Character can receives all items or not?
            BaseItem.GetDismantleReturnItems(nonEquipItem, amount, out returningItems, out returningCurrencies);
            if (simulatingNonEquipItems.IncreasingItemsWillOverwhelming(
                returningItems,
                GameInstance.Singleton.IsLimitInventoryWeight,
                character.GetCaches().LimitItemWeight,
                character.GetCaches().TotalItemWeight,
                GameInstance.Singleton.IsLimitInventorySlot,
                character.GetCaches().LimitItemSlot))
            {
                returningItems.Clear();
                gameMessage = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                return false;
            }

            simulatingNonEquipItems.IncreaseItems(returningItems);
            returningGold = nonEquipItem.GetItem().DismantleReturnGold * amount;
            return true;
        }

        public static bool DismantleItem(this IPlayerCharacterData character, int index, int amount, out UITextKeys gameMessage)
        {
#if UNITY_EDITOR || UNITY_SERVER
            int returningGold;
            List<ItemAmount> returningItems;
            List<CurrencyAmount> returningCurrencies;
            List<CharacterItem> simulatingNonEquipItems = character.NonEquipItems.Clone();
            if (!character.VerifyDismantleItem(index, amount, simulatingNonEquipItems, out gameMessage, out returningGold, out returningItems, out returningCurrencies))
                return false;

            character.Gold = character.Gold.Increase(returningGold);
            character.DecreaseItemsByIndex(index, amount, true);
            character.IncreaseItems(returningItems);
            character.IncreaseCurrencies(returningCurrencies);
            character.FillEmptySlots();
            return true;
#else
            gameMessage = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
            return false;
#endif
        }

        public static bool DismantleItems(this IPlayerCharacterData character, int[] selectedIndexes, out UITextKeys gameMessage)
        {
#if UNITY_EDITOR || UNITY_SERVER
            gameMessage = UITextKeys.NONE;
            List<int> indexes = new List<int>(selectedIndexes);
            indexes.Sort();
            Dictionary<int, int> indexAmountPairs = new Dictionary<int, int>();
            List<CharacterItem> simulatingNonEquipItems = character.NonEquipItems.Clone();
            int returningGold = 0;
            List<ItemAmount> returningItems = new List<ItemAmount>();
            List<CurrencyAmount> returningCurrencies = new List<CurrencyAmount>();
            int tempIndex;
            int tempAmount;
            int tempReturningGold;
            List<ItemAmount> tempReturningItems;
            List<CurrencyAmount> tempReturningCurrencies;
            for (int i = indexes.Count - 1; i >= 0; --i)
            {
                tempIndex = indexes[i];
                if (indexAmountPairs.ContainsKey(tempIndex))
                    continue;
                if (tempIndex >= character.NonEquipItems.Count)
                    continue;
                tempAmount = character.NonEquipItems[tempIndex].amount;
                if (!character.VerifyDismantleItem(tempIndex, tempAmount, simulatingNonEquipItems, out gameMessage, out tempReturningGold, out tempReturningItems, out tempReturningCurrencies))
                    return false;
                returningGold += tempReturningGold;
                returningItems.AddRange(tempReturningItems);
                returningCurrencies.AddRange(tempReturningCurrencies);
                indexAmountPairs.Add(tempIndex, tempAmount);
            }
            character.Gold = character.Gold.Increase(returningGold);
            indexes.Clear();
            indexes.AddRange(indexAmountPairs.Keys);
            indexes.Sort();
            for (int i = indexes.Count - 1; i >= 0; --i)
            {
                character.DecreaseItemsByIndex(indexes[i], indexAmountPairs[indexes[i]], true);
            }
            character.IncreaseItems(returningItems);
            character.IncreaseCurrencies(returningCurrencies);
            character.FillEmptySlots();
            return true;
#else
            gameMessage = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
            return false;
#endif
        }

        public static bool RefineItem(this IPlayerCharacterData character, InventoryType inventoryType, int index, int[] enhancerDataIds, out UITextKeys gameMessage)
        {
#if UNITY_EDITOR || UNITY_SERVER
            switch (inventoryType)
            {
                case InventoryType.NonEquipItems:
                    return BaseItem.RefineNonEquipItem(character, index, enhancerDataIds, out gameMessage);
                case InventoryType.EquipItems:
                    return BaseItem.RefineEquipItem(character, index, enhancerDataIds, out gameMessage);
                case InventoryType.EquipWeaponRight:
                    return BaseItem.RefineRightHandItem(character, enhancerDataIds, out gameMessage);
                case InventoryType.EquipWeaponLeft:
                    return BaseItem.RefineLeftHandItem(character, enhancerDataIds, out gameMessage);
            }
            gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_DATA;
            return false;
#else
            gameMessage = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
            return false;
#endif
        }

        public static bool EnhanceSocketItem(this IPlayerCharacterData character, InventoryType inventoryType, int index, int enhancerId, int socketIndex, out UITextKeys gameMessage)
        {
#if UNITY_EDITOR || UNITY_SERVER
            switch (inventoryType)
            {
                case InventoryType.NonEquipItems:
                    return BaseItem.EnhanceSocketNonEquipItem(character, index, enhancerId, socketIndex, out gameMessage);
                case InventoryType.EquipItems:
                    return BaseItem.EnhanceSocketEquipItem(character, index, enhancerId, socketIndex, out gameMessage);
                case InventoryType.EquipWeaponRight:
                    return BaseItem.EnhanceSocketRightHandItem(character, enhancerId, socketIndex, out gameMessage);
                case InventoryType.EquipWeaponLeft:
                    return BaseItem.EnhanceSocketLeftHandItem(character, enhancerId, socketIndex, out gameMessage);
            }
            gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_DATA;
            return false;
#else
            gameMessage = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
            return false;
#endif
        }

        public static bool RemoveEnhancerFromItem(this IPlayerCharacterData character, InventoryType inventoryType, int index, int socketIndex, out UITextKeys gameMessage)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!GameInstance.Singleton.enhancerRemoval.CanRemove(character, out gameMessage))
                return false;
            bool returnEnhancer = GameInstance.Singleton.enhancerRemoval.ReturnEnhancerItem;
            switch (inventoryType)
            {
                case InventoryType.NonEquipItems:
                    return BaseItem.RemoveEnhancerFromNonEquipItem(character, index, socketIndex, returnEnhancer, out gameMessage);
                case InventoryType.EquipItems:
                    return BaseItem.RemoveEnhancerFromEquipItem(character, index, socketIndex, returnEnhancer, out gameMessage);
                case InventoryType.EquipWeaponRight:
                    return BaseItem.RemoveEnhancerFromRightHandItem(character, socketIndex, returnEnhancer, out gameMessage);
                case InventoryType.EquipWeaponLeft:
                    return BaseItem.RemoveEnhancerFromLeftHandItem(character, socketIndex, returnEnhancer, out gameMessage);
            }
            gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_DATA;
            return false;
#else
            gameMessage = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
            return false;
#endif
        }

        public static bool RepairItem(this IPlayerCharacterData character, InventoryType inventoryType, int index, out UITextKeys gameMessage)
        {
#if UNITY_EDITOR || UNITY_SERVER
            switch (inventoryType)
            {
                case InventoryType.NonEquipItems:
                    return BaseItem.RepairNonEquipItem(character, index, out gameMessage);
                case InventoryType.EquipItems:
                    return BaseItem.RepairEquipItem(character, index, out gameMessage);
                case InventoryType.EquipWeaponRight:
                    return BaseItem.RepairRightHandItem(character, out gameMessage);
                case InventoryType.EquipWeaponLeft:
                    return BaseItem.RepairLeftHandItem(character, out gameMessage);
            }
            gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_DATA;
            return false;
#else
            gameMessage = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
            return false;
#endif
        }

        public static bool RepairEquipItems(this IPlayerCharacterData character, out UITextKeys gameMessage)
        {
#if UNITY_EDITOR || UNITY_SERVER
            bool success = false;
            BaseItem.RepairRightHandItem(character, out gameMessage);
            success = success || gameMessage == UITextKeys.UI_REPAIR_SUCCESS;
            BaseItem.RepairLeftHandItem(character, out gameMessage);
            success = success || gameMessage == UITextKeys.UI_REPAIR_SUCCESS;
            for (int i = 0; i < character.EquipItems.Count; ++i)
            {
                BaseItem.RepairEquipItem(character, i, out gameMessage);
                success = success || gameMessage == UITextKeys.UI_REPAIR_SUCCESS;
            }
            // Will send messages to inform that it can repair an items when any item can be repaired
            if (success)
                gameMessage = UITextKeys.UI_REPAIR_SUCCESS;
            else
                gameMessage = UITextKeys.UI_ERROR_CANNOT_REPAIR;

            return success;
#else
            gameMessage = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
            return false;
#endif
        }

        public static bool SellItem(this IPlayerCharacterData character, int index, int amount, out UITextKeys gameMessage)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (index < 0 || index >= character.NonEquipItems.Count)
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                return false;
            }

            // Found selling item or not?
            CharacterItem nonEquipItem = character.NonEquipItems[index];
            if (nonEquipItem.IsEmptySlot() || amount > nonEquipItem.amount)
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS;
                return false;
            }

            // Remove item from inventory
            character.DecreaseItemsByIndex(index, amount, true);
            character.FillEmptySlots();

            // Increase currencies
            BaseItem item = nonEquipItem.GetItem();
            GameInstance.Singleton.GameplayRule.IncreaseCurrenciesWhenSellItem(character, item, amount);
            gameMessage = UITextKeys.NONE;
            return true;
#else
            gameMessage = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
            return false;
#endif
        }

        public static bool SellItems(this IPlayerCharacterData character, int[] selectedIndexes, out UITextKeys gameMessage)
        {
#if UNITY_EDITOR || UNITY_SERVER
            List<int> indexes = new List<int>(selectedIndexes);
            indexes.Sort();
            int tempIndex;
            for (int i = indexes.Count - 1; i >= 0; --i)
            {
                tempIndex = indexes[i];
                if (tempIndex >= character.NonEquipItems.Count)
                    continue;
                character.SellItem(tempIndex, character.NonEquipItems[tempIndex].amount, out _);
            }
            gameMessage = UITextKeys.NONE;
            return true;
#else
            gameMessage = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
            return false;
#endif
        }
    }
}
