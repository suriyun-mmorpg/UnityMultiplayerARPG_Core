using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class UICharacterItemsUtils
    {
        public static List<KeyValuePair<int, CharacterItem>> GetFilteredList(List<CharacterItem> list, List<string> filterCategories, List<ItemType> filterItemTypes, bool doNotShowEmptySlots)
        {
            // Prepare result
            List<KeyValuePair<int, CharacterItem>> result = new List<KeyValuePair<int, CharacterItem>>();
            // Trim filter categories
            for (int i = 0; i < filterCategories.Count; ++i)
            {
                filterCategories[i] = filterCategories[i].Trim().ToLower();
            }
            CharacterItem entry;
            BaseItem tempItem;
            for (int i = 0; i < list.Count; ++i)
            {
                entry = list[i];
                if (entry.IsEmptySlot() && (!GameInstance.Singleton.IsLimitInventorySlot || doNotShowEmptySlots ||
                    (filterCategories != null && filterCategories.Count > 0) ||
                    (filterItemTypes != null && filterItemTypes.Count > 0)))
                {
                    // Hide empty slot
                    continue;
                }
                tempItem = entry.GetItem();
                if (tempItem == null)
                {
                    // Add empty slots
                    result.Add(new KeyValuePair<int, CharacterItem>(i, entry));
                    continue;
                }
                if (filterCategories.Count > 0 && !string.IsNullOrEmpty(tempItem.Category) && !filterCategories.Contains(tempItem.Category.Trim().ToLower()))
                {
                    // Category filtering
                    continue;
                }
                if (filterItemTypes.Count > 0 && !filterItemTypes.Contains(tempItem.ItemType))
                {
                    // Item type filtering
                    continue;
                }
                result.Add(new KeyValuePair<int, CharacterItem>(i, entry));
            }
            return result;
        }
    }
}
