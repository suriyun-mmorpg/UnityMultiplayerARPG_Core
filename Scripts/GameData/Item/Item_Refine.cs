using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class Item
    {
        public bool CanRefine(IPlayerCharacterData character, short level)
        {
            GameMessage.Type gameMessageType;
            return CanRefine(character, level, out gameMessageType);
        }

        public bool CanRefine(IPlayerCharacterData character, short level, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.CannotRefine;
            if (!IsEquipment())
            {
                // Cannot refine because it's not equipment item
                return false;
            }
            if (itemRefine == null)
            {
                // Cannot refine because there is no item refine info
                return false;
            }
            if (level >= itemRefine.levels.Length)
            {
                // Cannot refine because item reached max level
                gameMessageType = GameMessage.Type.RefineItemReachedMaxLevel;
                return false;
            }
            return itemRefine.levels[level - 1].CanRefine(character, out gameMessageType);
        }

        public static void RefineRightHandItem(IPlayerCharacterData character, out GameMessage.Type gameMessageType)
        {
            RefineItem(character, character.EquipWeapons.rightHand, (refinedItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.rightHand = refinedItem;
                character.EquipWeapons = equipWeapon;
            }, () =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.rightHand = CharacterItem.Empty;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static void RefineLeftHandItem(IPlayerCharacterData character, out GameMessage.Type gameMessageType)
        {
            RefineItem(character, character.EquipWeapons.leftHand, (refinedItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.leftHand = refinedItem;
                character.EquipWeapons = equipWeapon;
            }, () =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.leftHand = CharacterItem.Empty;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static void RefineEquipItem(IPlayerCharacterData character, int index, out GameMessage.Type gameMessageType)
        {
            RefineItemByList(character, character.EquipItems, index, out gameMessageType);
        }

        public static void RefineNonEquipItem(IPlayerCharacterData character, int index, out GameMessage.Type gameMessageType)
        {
            RefineItemByList(character, character.NonEquipItems, index, out gameMessageType);
        }

        private static void RefineItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int index, out GameMessage.Type gameMessageType)
        {
            RefineItem(character, list[index], (refinedItem) =>
            {
                list[index] = refinedItem;
            }, () =>
            {
                list.RemoveAt(index);
            }, out gameMessageType);
        }

        private static void RefineItem(IPlayerCharacterData character, CharacterItem refiningItem, System.Action<CharacterItem> onRefine, System.Action onDestroy, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.CannotRefine;
            if (refiningItem.IsEmptySlot())
            {
                // Cannot refine because character item is empty
                return;
            }
            Item equipmentItem = refiningItem.GetEquipmentItem();
            if (equipmentItem == null)
            {
                // Cannot refine because it's not equipment item
                return;
            }
            if (!equipmentItem.CanRefine(character, refiningItem.level, out gameMessageType))
            {
                // Cannot refine because of some reasons
                return;
            }
            ItemRefineLevel refineLevel = equipmentItem.itemRefine.levels[refiningItem.level - 1];
            if (Random.value <= refineLevel.SuccessRate)
            {
                // If success, increase item level
                gameMessageType = GameMessage.Type.RefineSuccess;
                ++refiningItem.level;
                onRefine.Invoke(refiningItem);
            }
            else
            {
                // Fail
                gameMessageType = GameMessage.Type.RefineFail;
                if (refineLevel.RefineFailDestroyItem)
                {
                    // If condition when fail is it has to be destroyed
                    onDestroy.Invoke();
                }
                else
                {
                    // If condition when fail is reduce its level
                    refiningItem.level -= refineLevel.RefineFailDecreaseLevels;
                    if (refiningItem.level < 1)
                        refiningItem.level = 1;
                    onRefine.Invoke(refiningItem);
                }
            }
            if (refineLevel.RequireItems != null)
            {
                // Decrease required items
                foreach (ItemAmount requireItem in refineLevel.RequireItems)
                {
                    if (requireItem.item != null && requireItem.amount > 0)
                        character.DecreaseItems(requireItem.item.DataId, requireItem.amount);
                }
            }
            // Decrease required gold
            GameInstance.Singleton.GameplayRule.DecreaseCurrenciesWhenRefineItem(character, refineLevel);
        }
    }
}