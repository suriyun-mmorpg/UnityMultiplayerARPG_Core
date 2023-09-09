using Cysharp.Text;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class ItemRandomBonusCacheManager
    {
        private static readonly Dictionary<string, ItemRandomBonusCache> s_caches = new Dictionary<string, ItemRandomBonusCache>();

        public static ItemRandomBonusCache GetCaches(this IEquipmentItem item, int randomSeed, byte version)
        {
            string key = ZString.Concat(item.DataId, '_', randomSeed, '_', version);
            if (!s_caches.ContainsKey(key))
                s_caches.Add(key, new ItemRandomBonusCache(item, randomSeed, version));
            return s_caches[key];
        }
    }
}
