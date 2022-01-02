using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class UICashPackagesUtils
    {
        public static List<CashPackage> GetFilteredList(List<CashPackage> list, List<string> filterCategories)
        {
            // Prepare result
            List<CashPackage> result = new List<CashPackage>();
            // Trim filter categories
            for (int i = 0; i < filterCategories.Count; ++i)
            {
                filterCategories[i] = filterCategories[i].Trim().ToLower();
            }
            CashPackage entry;
            for (int i = 0; i < list.Count; ++i)
            {
                entry = list[i];
                if (entry == null)
                {
                    // Skip empty data
                    continue;
                }
                if (!string.IsNullOrEmpty(entry.Category) && !filterCategories.Contains(entry.Category.Trim().ToLower()))
                {
                    // Category filtering
                    continue;
                }
                result.Add(entry);
            }
            return result;
        }
    }
}
