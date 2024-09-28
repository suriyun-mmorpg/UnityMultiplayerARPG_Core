using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static partial class CharacterInventoryExtensions
    {
        public static bool RepairItem(this IPlayerCharacterData character, InventoryType inventoryType, int index, out UITextKeys gameMessage)
        {
#if UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES
            switch (inventoryType)
            {
                case InventoryType.NonEquipItems:
                    return character.RepairNonEquipItem(index, out gameMessage);
                case InventoryType.EquipItems:
                    return character.RepairEquipItem(index, out gameMessage);
                case InventoryType.EquipWeaponRight:
                    return character.RepairRightHandItem(out gameMessage);
                case InventoryType.EquipWeaponLeft:
                    return character.RepairLeftHandItem(out gameMessage);
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
#if UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES
            bool success = false;
            character.RepairRightHandItem(out gameMessage);
            success = success || gameMessage == UITextKeys.UI_REPAIR_SUCCESS;
            character.RepairLeftHandItem(out gameMessage);
            success = success || gameMessage == UITextKeys.UI_REPAIR_SUCCESS;
            for (int i = 0; i < character.EquipItems.Count; ++i)
            {
                character.RepairEquipItem(i, out gameMessage);
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

        public static bool RepairRightHandItem(this IPlayerCharacterData character, out UITextKeys gameMessageType)
        {
            return RepairItem(character, character.EquipWeapons.rightHand, (repairedItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.rightHand = repairedItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static bool RepairLeftHandItem(this IPlayerCharacterData character, out UITextKeys gameMessageType)
        {
            return RepairItem(character, character.EquipWeapons.leftHand, (repairedItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.leftHand = repairedItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static bool RepairEquipItem(this IPlayerCharacterData character, int index, out UITextKeys gameMessageType)
        {
            return RepairItemByList(character, character.EquipItems, index, out gameMessageType);
        }

        public static bool RepairNonEquipItem(this IPlayerCharacterData character, int index, out UITextKeys gameMessageType)
        {
            return RepairItemByList(character, character.NonEquipItems, index, out gameMessageType);
        }

        private static bool RepairItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int index, out UITextKeys gameMessageType)
        {
            return RepairItem(character, list[index], (repairedItem) =>
            {
                list[index] = repairedItem;
            }, out gameMessageType);
        }

        private static bool RepairItem(IPlayerCharacterData character, CharacterItem repairingItem, System.Action<CharacterItem> onRepaired, out UITextKeys gameMessageType)
        {
            if (repairingItem.IsEmptySlot())
            {
                // Cannot refine because character item is empty
                gameMessageType = UITextKeys.UI_ERROR_ITEM_NOT_FOUND;
                return false;
            }
            BaseItem equipmentItem = repairingItem.GetItem();
            if (!equipmentItem.CanRepair(character, repairingItem.durability, out float maxDurability, out ItemRepairPrice repairPrice, out gameMessageType))
                return false;
            gameMessageType = UITextKeys.UI_REPAIR_SUCCESS;
            // Repair item
            repairingItem.durability = maxDurability;
            onRepaired.Invoke(repairingItem);
            if (repairPrice.RequireItems != null)
            {
                // Decrease required items
                character.DecreaseItems(repairPrice.RequireItems);
                character.FillEmptySlots();
            }
            // Decrease required gold
            GameInstance.Singleton.GameplayRule.DecreaseCurrenciesWhenRepairItem(character, repairPrice);
            GameInstance.ServerLogHandlers.LogRepair(character, repairingItem, repairPrice);
            return true;
        }
    }
}
