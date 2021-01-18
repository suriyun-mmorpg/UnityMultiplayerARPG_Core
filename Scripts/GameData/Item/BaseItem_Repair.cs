using System.Collections.Generic;
using UnityEngine;

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
            repairPrice = default(ItemRepairPrice);
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
            return repairPrice.CanRepair(character, out gameMessageType);
        }

        public ItemRepairPrice GetRepairPrice(float durability)
        {
            return GetRepairPrice(durability, out _);
        }

        public ItemRepairPrice GetRepairPrice(float durability, out float maxDurability)
        {
            ItemRepairPrice repairPrice = default(ItemRepairPrice);
            maxDurability = (this as IEquipmentItem).MaxDurability;
            if (maxDurability <= 0f)
                return repairPrice;
            float durabilityRate = durability / maxDurability;
            if (durabilityRate >= 0.99f)
                return repairPrice;
            for (int i = 0; i < itemRefine.repairPrices.Length; ++i)
            {
                repairPrice = itemRefine.repairPrices[i];
                if (durabilityRate < repairPrice.DurabilityRate)
                    return repairPrice;
            }
            return repairPrice;
        }

        public static void RepairRightHandItem(IPlayerCharacterData character, out UITextKeys gameMessageType)
        {
            RepairItem(character, character.EquipWeapons.rightHand, (repairedItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.rightHand = repairedItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static void RepairLeftHandItem(IPlayerCharacterData character, out UITextKeys gameMessageType)
        {
            RepairItem(character, character.EquipWeapons.leftHand, (repairedItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.leftHand = repairedItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static void RepairEquipItem(IPlayerCharacterData character, int index, out UITextKeys gameMessageType)
        {
            RepairItemByList(character, character.EquipItems, index, out gameMessageType);
        }

        public static void RepairNonEquipItem(IPlayerCharacterData character, int index, out UITextKeys gameMessageType)
        {
            RepairItemByList(character, character.NonEquipItems, index, out gameMessageType);
        }

        private static void RepairItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int index, out UITextKeys gameMessageType)
        {
            RepairItem(character, list[index], (repairedItem) =>
            {
                list[index] = repairedItem;
            }, out gameMessageType);
        }

        private static void RepairItem(IPlayerCharacterData character, CharacterItem repairingItem, System.Action<CharacterItem> onRepaired, out UITextKeys gameMessageType)
        {
            gameMessageType = UITextKeys.UI_ERROR_CANNOT_REPAIR;
            if (repairingItem.IsEmptySlot())
            {
                // Cannot refine because character item is empty
                return;
            }
            BaseItem equipmentItem = repairingItem.GetEquipmentItem() as BaseItem;
            if (equipmentItem == null)
            {
                // Cannot refine because it's not equipment item
                return;
            }
            float maxDurability;
            ItemRepairPrice repairPrice;
            if (equipmentItem.CanRepair(character, repairingItem.durability, out maxDurability, out repairPrice, out gameMessageType))
            {
                gameMessageType = UITextKeys.UI_REPAIR_SUCCESS;
                // Repair item
                repairingItem.durability = maxDurability;
                onRepaired.Invoke(repairingItem);
                // Decrease required gold
                GameInstance.Singleton.GameplayRule.DecreaseCurrenciesWhenRepairItem(character, repairPrice);
            }
        }
    }
}
