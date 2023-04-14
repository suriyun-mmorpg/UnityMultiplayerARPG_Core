using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class ItemRandomBonusCacheManager
    {
        private static readonly Dictionary<int, Dictionary<int, ItemRandomBonusCache>> s_caches = new Dictionary<int, Dictionary<int, ItemRandomBonusCache>>();

        public static ItemRandomBonusCache GetCaches(this IEquipmentItem item, int randomSeed)
        {
            if (!s_caches.ContainsKey(item.DataId))
                s_caches.Add(item.DataId, new Dictionary<int, ItemRandomBonusCache>());
            if (!s_caches[item.DataId].ContainsKey(randomSeed))
                s_caches[item.DataId].Add(randomSeed, new ItemRandomBonusCache(item, randomSeed));
            return s_caches[item.DataId][randomSeed];
        }
    }
}
