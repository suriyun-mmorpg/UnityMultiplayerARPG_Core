using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class UICharacterQuestsUtils
    {
        public static List<CharacterQuest> GetFilteredList(IList<CharacterQuest> list, bool showOnlyTrackingQuests, bool showAllWhenNoTrackedQuests, bool hideCompleteQuest, List<string> filterCategories)
        {
            // Prepare result
            List<CharacterQuest> result = new List<CharacterQuest>();
            bool hasTrackingQuests = false;
            CharacterQuest entry;

            // Convert filter categories to lowercase and trim them
            for (int i = 0; i < filterCategories.Count; ++i)
            {
                filterCategories[i] = filterCategories[i].Trim().ToLower();
            }

            // Check if there are any tracking quests
            if (showOnlyTrackingQuests)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    entry = list[i];
                    if (!GameInstance.Quests.ContainsKey(entry.dataId))
                        continue;
                    if (entry.isTracking)
                    {
                        hasTrackingQuests = true;
                        break;
                    }
                }
            }

            // Filter quests based on criteria
            for (int i = 0; i < list.Count; ++i)
            {
                entry = list[i];
                if (!GameInstance.Quests.TryGetValue(entry.dataId, out Quest quest))
                    continue;

                // Filter by category
                if (filterCategories.Count > 0 && (quest.Category == null || !filterCategories.Contains(quest.Category.Trim().ToLower())))
                    continue;

                // Filter by tracking status
                if (showOnlyTrackingQuests && !entry.isTracking && (!showAllWhenNoTrackedQuests || hasTrackingQuests))
                    continue;

                // Filter completed quests
                if (hideCompleteQuest && entry.isComplete)
                    continue;

                result.Add(entry);
            }

            return result;
        }
    }
}
