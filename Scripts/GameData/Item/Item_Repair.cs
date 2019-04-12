using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class Item
    {
        public bool CanRepair(IPlayerCharacterData character, float durability, out int requireGold)
        {
            GameMessage.Type gameMessageType;
            return CanRepair(character, durability, out requireGold, out gameMessageType);
        }

        public bool CanRepair(IPlayerCharacterData character, float durability, out int requireGold, out GameMessage.Type gameMessageType)
        {
            requireGold = 0;
            gameMessageType = GameMessage.Type.CannotRepair;
            if (!IsEquipment())
            {
                // Cannot repair because it's not equipment item
                return false;
            }
            if (itemRefineInfo == null)
            {
                // Cannot repair because there is no item refine info
                return false;
            }
            float durabilityRate = durability / maxDurability;
            foreach (ItemRepairPrice repairPrice in itemRefineInfo.repairPrices)
            {
                if (durabilityRate < repairPrice.DurabilityRate)
                {
                    requireGold = repairPrice.RequireGold;
                    return repairPrice.CanRepair(character, out gameMessageType);
                }
            }
            return true;
        }

        public static void RepairRightHandItem(IPlayerCharacterData character, out GameMessage.Type gameMessageType)
        {
            RepairItem(character, character.EquipWeapons.rightHand, (repairedItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.rightHand = repairedItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static void RepairLeftHandItem(IPlayerCharacterData character, out GameMessage.Type gameMessageType)
        {
            RepairItem(character, character.EquipWeapons.leftHand, (repairedItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.leftHand = repairedItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static void RepairEquipItem(IPlayerCharacterData character, int index, out GameMessage.Type gameMessageType)
        {
            RepairItemByList(character, character.EquipItems, index, out gameMessageType);
        }

        public static void RepairNonEquipItem(IPlayerCharacterData character, int index, out GameMessage.Type gameMessageType)
        {
            RepairItemByList(character, character.NonEquipItems, index, out gameMessageType);
        }

        private static void RepairItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int index, out GameMessage.Type gameMessageType)
        {
            RepairItem(character, list[index], (repairedItem) =>
            {
                list[index] = repairedItem;
            }, out gameMessageType);
        }

        private static void RepairItem(IPlayerCharacterData character, CharacterItem repairingItem, System.Action<CharacterItem> onRepaired, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.CannotRepair;
            if (!repairingItem.NotEmptySlot())
            {
                // Cannot refine because character item is empty
                return;
            }
            Item equipmentItem = repairingItem.GetEquipmentItem();
            if (equipmentItem == null)
            {
                // Cannot refine because it's not equipment item
                return;
            }
            int requireGold = 0;
            if (equipmentItem.CanRepair(character, repairingItem.durability, out requireGold, out gameMessageType))
            {
                gameMessageType = GameMessage.Type.RepairSuccess;
                // Repair item
                repairingItem.durability = equipmentItem.maxDurability;
                onRepaired.Invoke(repairingItem);
                // Decrease required gold
                character.Gold -= requireGold;
            }
        }
    }
}