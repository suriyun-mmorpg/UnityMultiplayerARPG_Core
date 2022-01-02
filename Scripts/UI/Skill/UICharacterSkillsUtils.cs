using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class UICharacterSkillsUtils
    {
        public static Dictionary<BaseSkill, short> GetFilteredList(Dictionary<BaseSkill, short> list, List<string> filterCategories, List<SkillType> filterSkillTypes)
        {
            // Prepare result
            Dictionary<BaseSkill, short> result = new Dictionary<BaseSkill, short>();
            // Trim filter categories
            for (int i = 0; i < filterCategories.Count; ++i)
            {
                filterCategories[i] = filterCategories[i].Trim().ToLower();
            }
            foreach (KeyValuePair<BaseSkill, short> kvp in list)
            {
                if (kvp.Key == null)
                {
                    // Skip empty data
                    continue;
                }
                if (filterCategories.Count > 0 && !string.IsNullOrEmpty(kvp.Key.Category) && !filterCategories.Contains(kvp.Key.Category.Trim().ToLower()))
                {
                    // Category filtering
                    continue;
                }
                if (filterSkillTypes.Count > 0 && !filterSkillTypes.Contains(kvp.Key.SkillType))
                {
                    // Skill type filtering
                    continue;
                }
                result.Add(kvp.Key, kvp.Value);
            }
            return result;
        }
    }
}
