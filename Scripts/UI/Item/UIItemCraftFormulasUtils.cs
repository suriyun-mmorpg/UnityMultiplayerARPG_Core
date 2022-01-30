using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class UIItemCraftFormulasUtils
    {
        public static List<ItemCraftFormula> GetFilteredList(List<ItemCraftFormula> list, List<string> filterCategories)
        {
            // Prepare result
            List<ItemCraftFormula> result = new List<ItemCraftFormula>();
            // Trim filter categories
            for (int i = 0; i < filterCategories.Count; ++i)
            {
                filterCategories[i] = filterCategories[i].Trim().ToLower();
            }
            ItemCraftFormula entry;
            for (int i = 0; i < list.Count; ++i)
            {
                entry = list[i];
                if (entry == null || entry.ItemCraft.CraftingItem == null)
                {
                    // Skip empty data
                    continue;
                }
                if (filterCategories.Count == 0 || (string.IsNullOrEmpty(entry.Category) && string.IsNullOrEmpty(entry.ItemCraft.CraftingItem.Category)) ||
                    (!string.IsNullOrEmpty(entry.Category) && filterCategories.Contains(entry.Category.Trim().ToLower())) ||
                    (!string.IsNullOrEmpty(entry.ItemCraft.CraftingItem.Category)) && filterCategories.Contains(entry.ItemCraft.CraftingItem.Category.Trim().ToLower()))
                {
                    // Category filtering
                    result.Add(entry);
                }
            }
            return result;
        }
    }
}
