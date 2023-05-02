using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class UICharacterQuestsUtils
    {
        public static List<CharacterQuest> GetFilteredList(IList<CharacterQuest> list, bool showOnlyTrackingQuests, bool showAllWhenNoTrackedQuests, bool hideCompleteQuest)
        {
            // Prepare result
            List<CharacterQuest> result = new List<CharacterQuest>();
            bool hasTrackingQuests = false;
            CharacterQuest entry;
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
            for (int i = 0; i < list.Count; ++i)
            {
                entry = list[i];
                if (!GameInstance.Quests.ContainsKey(entry.dataId))
                    continue;
                if (showOnlyTrackingQuests && !entry.isTracking && (!showAllWhenNoTrackedQuests || hasTrackingQuests))
                    continue;
                if (hideCompleteQuest && entry.isComplete)
                    continue;
                result.Add(entry);
            }
            return result;
        }
    }
}
