using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class ItemRandomBonusCacheManager
    {
        private static readonly Dictionary<int, Dictionary<int, ItemRandomBonusCache>> caches = new Dictionary<int, Dictionary<int, ItemRandomBonusCache>>();

        public static ItemRandomBonusCache GetCaches(this IEquipmentItem item, int randomSeed)
        {
            if (!caches.ContainsKey(item.DataId))
                caches.Add(item.DataId, new Dictionary<int, ItemRandomBonusCache>());
            if (!caches[item.DataId].ContainsKey(randomSeed))
                caches[item.DataId].Add(randomSeed, new ItemRandomBonusCache(item, randomSeed));
            return caches[item.DataId][randomSeed];
        }
    }
}
