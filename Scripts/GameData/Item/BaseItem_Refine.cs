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

        public bool CanRefine(IPlayerCharacterData character, short level, out UITextKeys gameMessage)
        {
            if (!this.IsEquipment())
            {
                // Cannot refine because it's not equipment item
                gameMessage = UITextKeys.UI_ERROR_ITEM_NOT_EQUIPMENT;
                return false;
            }
            if (ItemRefine == null)
            {
                // Cannot refine because there is no item refine info
                gameMessage = UITextKeys.UI_ERROR_CANNOT_REFINE;
                return false;
            }
            if (level >= ItemRefine.Levels.Length)
            {
                // Cannot refine because item reached max level
                gameMessage = UITextKeys.UI_ERROR_REFINE_ITEM_REACHED_MAX_LEVEL;
                return false;
            }
            return ItemRefine.Levels[level - 1].CanRefine(character, out gameMessage);
        }

        public static bool RefineRightHandItem(IPlayerCharacterData character, out UITextKeys gameMessageType)
        {
            return RefineItem(character, character.EquipWeapons.rightHand, (refinedItem) =>
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

        public static bool RefineLeftHandItem(IPlayerCharacterData character, out UITextKeys gameMessageType)
        {
            return RefineItem(character, character.EquipWeapons.leftHand, (refinedItem) =>
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

        public static bool RefineEquipItem(IPlayerCharacterData character, int index, out UITextKeys gameMessageType)
        {
            return RefineItemByList(character, character.EquipItems, index, out gameMessageType);
        }

        public static bool RefineNonEquipItem(IPlayerCharacterData character, int index, out UITextKeys gameMessageType)
        {
            return RefineItemByList(character, character.NonEquipItems, index, out gameMessageType);
        }

        private static bool RefineItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int index, out UITextKeys gameMessageType)
        {
            return RefineItem(character, list[index], (refinedItem) =>
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

        private static bool RefineItem(IPlayerCharacterData character, CharacterItem refiningItem, System.Action<CharacterItem> onRefine, System.Action onDestroy, out UITextKeys gameMessage)
        {
            if (refiningItem.IsEmptySlot())
            {
                // Cannot refine because character item is empty
                gameMessage = UITextKeys.UI_ERROR_ITEM_NOT_FOUND;
                return false;
            }
            BaseItem equipmentItem = refiningItem.GetEquipmentItem() as BaseItem;
            if (equipmentItem == null)
            {
                // Cannot refine because it's not equipment item
                gameMessage = UITextKeys.UI_ERROR_ITEM_NOT_EQUIPMENT;
                return false;
            }
            if (!equipmentItem.CanRefine(character, refiningItem.level, out gameMessage))
            {
                // Cannot refine because of some reasons
                return false;
            }
            ItemRefineLevel refineLevel = equipmentItem.ItemRefine.Levels[refiningItem.level - 1];
            if (Random.value <= refineLevel.SuccessRate)
            {
                // If success, increase item level
                gameMessage = UITextKeys.UI_REFINE_SUCCESS;
                ++refiningItem.level;
                onRefine.Invoke(refiningItem);
            }
            else
            {
                // Fail
                gameMessage = UITextKeys.UI_REFINE_FAIL;
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
                character.DecreaseItems(refineLevel.RequireItems);
                character.FillEmptySlots();
            }
            if (refineLevel.RequireCurrencies != null)
            {
                // Decrease required currencies
                character.DecreaseCurrencies(refineLevel.RequireCurrencies);
            }
            // Decrease required gold
            GameInstance.Singleton.GameplayRule.DecreaseCurrenciesWhenRefineItem(character, refineLevel);
            return true;
        }
    }
}
