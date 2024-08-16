using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseItem
    {
        public bool TryGetItemRefineLevel(int level, out ItemRefineLevel refineLevel)
        {
            refineLevel = default;
            if (ItemRefine == null)
                return false;
            if (level - 1 >= ItemRefine.Levels.Length)
                return false;
            refineLevel = ItemRefine.Levels[level - 1];
            return true;
        }

        public bool CanRefine(IPlayerCharacterData character, int level, int[] enhancerDataIds)
        {
            return CanRefine(character, level, enhancerDataIds, out _);
        }

        public bool CanRefine(IPlayerCharacterData character, int level, int[] enhancerDataIds, out UITextKeys gameMessage)
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
            if (level - 1 >= ItemRefine.Levels.Length)
            {
                // Cannot refine because item reached max level
                gameMessage = UITextKeys.UI_ERROR_REFINE_ITEM_REACHED_MAX_LEVEL;
                return false;
            }
            if (GameInstance.Singleton.refineEnhancerItemsLimit > 0 && enhancerDataIds.Length > GameInstance.Singleton.refineEnhancerItemsLimit)
            {
                // Cannot refine because enhancer items reached level
                gameMessage = UITextKeys.UI_ERROR_REACHED_REFINE_ENHANCER_ITEMS_LIMIT;
                return false;
            }
            return ItemRefine.Levels[level - 1].CanRefine(character, enhancerDataIds, out gameMessage);
        }

        public static bool RefineRightHandItem(IPlayerCharacterData character, int[] enhancerDataIds, out UITextKeys gameMessageType)
        {
            return RefineItem(character, character.EquipWeapons.rightHand, enhancerDataIds, (refinedItem) =>
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

        public static bool RefineLeftHandItem(IPlayerCharacterData character, int[] enhancerDataIds, out UITextKeys gameMessageType)
        {
            return RefineItem(character, character.EquipWeapons.leftHand, enhancerDataIds, (refinedItem) =>
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

        public static bool RefineEquipItem(IPlayerCharacterData character, int itemIndex, int[] enhancerDataIds, out UITextKeys gameMessageType)
        {
            return RefineItemByList(character, character.EquipItems, itemIndex, enhancerDataIds, out gameMessageType);
        }

        public static bool RefineNonEquipItem(IPlayerCharacterData character, int itemIndex, int[] enhancerDataIds, out UITextKeys gameMessageType)
        {
            return RefineItemByList(character, character.NonEquipItems, itemIndex, enhancerDataIds, out gameMessageType);
        }

        private static bool RefineItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int itemIndex, int[] enhancerDataIds, out UITextKeys gameMessageType)
        {
            return RefineItem(character, list[itemIndex], enhancerDataIds, (refinedItem) =>
            {
                list[itemIndex] = refinedItem;
            }, () =>
            {
                list.RemoveOrPlaceEmptySlot(itemIndex);
            }, out gameMessageType);
        }

        private static bool RefineItem(IPlayerCharacterData character, CharacterItem refiningItem, int[] enhancerDataIds, System.Action<CharacterItem> onRefine, System.Action onDestroy, out UITextKeys gameMessage)
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
            if (!equipmentItem.CanRefine(character, refiningItem.level, enhancerDataIds, out gameMessage))
            {
                // Cannot refine because of some reasons
                return false;
            }
            ItemRefineLevel refineLevel = equipmentItem.ItemRefine.Levels[refiningItem.level - 1];
            bool inventoryChanged = false;
            List<BaseItem> enhancerItems = new List<BaseItem>();
            float increaseSuccessRate = 0f;
            float decreaseRequireGoldRate = 0f;
            float chanceToNotDecreaseLevels = 0f;
            float chanceToNotDestroyItem = 0f;
            for (int i = 0; i < enhancerDataIds.Length; ++i)
            {
                int materialDataId = enhancerDataIds[i];
                int indexOfMaterial = character.IndexOfNonEquipItem(materialDataId);
                if (indexOfMaterial < 0 || character.NonEquipItems[indexOfMaterial].IsEmptySlot())
                {
                    // Material not found
                    continue;
                }
                for (int j = 0; j < refineLevel.AvailableEnhancers.Length; ++j)
                {
                    if (refineLevel.AvailableEnhancers[j].item == null || refineLevel.AvailableEnhancers[j].item.DataId != materialDataId)
                    {
                        // Not a material we're looking for
                        continue;
                    }
                    // Found the material, enhance
                    increaseSuccessRate += refineLevel.AvailableEnhancers[j].increaseSuccessRate;
                    decreaseRequireGoldRate += refineLevel.AvailableEnhancers[j].decreaseRequireGoldRate;
                    chanceToNotDecreaseLevels += refineLevel.AvailableEnhancers[j].chanceToNotDecreaseLevels;
                    chanceToNotDestroyItem += refineLevel.AvailableEnhancers[j].chanceToNotDestroyItem;
                    break;
                }
                enhancerItems.Add(character.NonEquipItems[indexOfMaterial].GetItem());
                character.DecreaseItems(materialDataId, 1);
                inventoryChanged = true;
            }
            bool isSuccess = false;
            bool isDestroy = false;
            int decreaseLevels = 0;
            bool isReturning = false;
            ItemRefineFailReturning returning = default;
            if (Random.value <= refineLevel.SuccessRate + increaseSuccessRate)
            {
                // If success, increase item level
                isSuccess = true;
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
                    if (Random.value > chanceToNotDestroyItem)
                    {
                        isDestroy = true;
                        onDestroy.Invoke();
                    }
                }
                else
                {
                    // If condition when fail is reduce its level
                    if (Random.value > chanceToNotDecreaseLevels)
                    {
                        decreaseLevels = refineLevel.RefineFailDecreaseLevels;
                        while (refiningItem.level - decreaseLevels < 1)
                            --decreaseLevels;
                        refiningItem.level -= decreaseLevels;
                        onRefine.Invoke(refiningItem);
                    }
                }
                // Return items/currencies
                if (refineLevel.FailReturnings != null && refineLevel.FailReturnings.Length > 0)
                {
                    Dictionary<ItemRefineFailReturning, int> randomItems = new Dictionary<ItemRefineFailReturning, int>();
                    foreach (ItemRefineFailReturning item in refineLevel.FailReturnings)
                    {
                        randomItems[item] = item.randomWeight;
                    }
                    returning = WeightedRandomizer.From(randomItems).TakeOne();
                    isReturning = true;
                    character.Gold = character.Gold.Increase(returning.returnGold);
                    character.IncreaseItems(returning.returnItems, onFail: dropData => ItemDropEntity.Drop(null, RewardGivenType.None, dropData, new string[] { character.Id }).Forget());
                    character.IncreaseCurrencies(returning.returnCurrencies);
                    inventoryChanged = true;
                }
            }
            if (refineLevel.RequireItems != null)
            {
                // Decrease required items
                character.DecreaseItems(refineLevel.RequireItems);
                inventoryChanged = true;
            }
            // Fill empty slots
            if (inventoryChanged)
                character.FillEmptySlots();
            // Decrease required gold
            GameInstance.Singleton.GameplayRule.DecreaseCurrenciesWhenRefineItem(character, refineLevel, decreaseRequireGoldRate);
            GameInstance.ServerLogHandlers.LogRefine(character, refiningItem, enhancerItems, increaseSuccessRate, decreaseRequireGoldRate, chanceToNotDecreaseLevels, chanceToNotDestroyItem, isSuccess, isDestroy, refineLevel, isReturning, returning);
            return true;
        }
    }
}
