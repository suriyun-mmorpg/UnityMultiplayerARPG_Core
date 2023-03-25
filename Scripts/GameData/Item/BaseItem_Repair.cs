using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class BaseItem
    {
        public bool CanRepair(IPlayerCharacterData character, float durability, out float maxDurability, out ItemRepairPrice repairPrice)
        {
            return CanRepair(character, durability, out maxDurability, out repairPrice, out _);
        }

        public bool CanRepair(IPlayerCharacterData character, float durability, out float maxDurability, out ItemRepairPrice repairPrice, out UITextKeys gameMessageType)
        {
            maxDurability = 0f;
            repairPrice = default;
            if (!this.IsEquipment())
            {
                // Cannot repair because it's not equipment item
                gameMessageType = UITextKeys.UI_ERROR_CANNOT_REPAIR;
                return false;
            }
            if (itemRefine == null)
            {
                // Cannot repair because there is no item refine info
                gameMessageType = UITextKeys.UI_ERROR_CANNOT_REPAIR;
                return false;
            }
            repairPrice = GetRepairPrice(durability, out maxDurability);
            if (durability >= maxDurability)
            {
                gameMessageType = UITextKeys.UI_ERROR_CANNOT_REPAIR;
                return false;
            }
            return repairPrice.CanRepair(character, out gameMessageType);
        }

        public ItemRepairPrice GetRepairPrice(float durability)
        {
            return GetRepairPrice(durability, out _);
        }

        public ItemRepairPrice GetRepairPrice(float durability, out float maxDurability)
        {
            ItemRepairPrice repairPrice = default;
            maxDurability = (this as IEquipmentItem).MaxDurability;
            if (maxDurability <= 0f)
                return repairPrice;
            float durabilityRate = durability / maxDurability;
            if (durabilityRate >= 1f)
                return repairPrice;
            System.Array.Sort(itemRefine.RepairPrices);
            for (int i = itemRefine.RepairPrices.Length - 1; i >= 0; --i)
            {
                repairPrice = itemRefine.RepairPrices[i];
                if (durabilityRate < repairPrice.DurabilityRate)
                    return repairPrice;
            }
            return repairPrice;
        }

        public static bool RepairRightHandItem(IPlayerCharacterData character, out UITextKeys gameMessageType)
        {
            return RepairItem(character, character.EquipWeapons.rightHand, (repairedItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.rightHand = repairedItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static bool RepairLeftHandItem(IPlayerCharacterData character, out UITextKeys gameMessageType)
        {
            return RepairItem(character, character.EquipWeapons.leftHand, (repairedItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.leftHand = repairedItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static bool RepairEquipItem(IPlayerCharacterData character, int index, out UITextKeys gameMessageType)
        {
            return RepairItemByList(character, character.EquipItems, index, out gameMessageType);
        }

        public static bool RepairNonEquipItem(IPlayerCharacterData character, int index, out UITextKeys gameMessageType)
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
            return true;
        }
    }
}
