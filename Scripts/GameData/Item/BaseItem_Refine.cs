using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseItem
    {
        public bool CanRefine(IPlayerCharacterData character, short level)
        {
            return CanRefine(character, level, out _);
        }

        public bool CanRefine(IPlayerCharacterData character, short level, out GameMessage.Type gameMessageType)
        {
            if (!this.IsEquipment())
            {
                // Cannot refine because it's not equipment item
                gameMessageType = GameMessage.Type.CannotRefine;
                return false;
            }
            if (ItemRefine == null)
            {
                // Cannot refine because there is no item refine info
                gameMessageType = GameMessage.Type.CannotRefine;
                return false;
            }
            if (level >= ItemRefine.levels.Length)
            {
                // Cannot refine because item reached max level
                gameMessageType = GameMessage.Type.RefineItemReachedMaxLevel;
                return false;
            }
            return ItemRefine.levels[level - 1].CanRefine(character, out gameMessageType);
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
                if (GameInstance.Singleton.IsLimitInventorySlot)
                    list[index] = CharacterItem.Empty;
                else
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
            BaseItem equipmentItem = refiningItem.GetEquipmentItem() as BaseItem;
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
            ItemRefineLevel refineLevel = equipmentItem.ItemRefine.levels[refiningItem.level - 1];
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
                        character.DecreaseItems(requireItem.item.DataId, requireItem.amount, GameInstance.Singleton.IsLimitInventorySlot);
                }
                character.FillEmptySlots();
            }
            // Decrease required gold
            GameInstance.Singleton.GameplayRule.DecreaseCurrenciesWhenRefineItem(character, refineLevel);
        }
    }
}
