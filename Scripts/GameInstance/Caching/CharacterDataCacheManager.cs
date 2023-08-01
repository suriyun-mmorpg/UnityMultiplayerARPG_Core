using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class CharacterDataCacheManager
    {
        private static readonly Dictionary<ICharacterData, CharacterDataCache> s_caches = new Dictionary<ICharacterData, CharacterDataCache>();

        public static CharacterDataCache GetCaches(this ICharacterData characterData)
        {
            if (characterData == null)
                return null;
            if (!s_caches.ContainsKey(characterData))
            {
                // Did not mark to mark cache yet, so mark it here before get caches
                return s_caches[characterData] = new CharacterDataCache().MarkToMakeCaches().GetCaches(characterData);
            }
            return s_caches[characterData].GetCaches(characterData);
        }

        public static CharacterDataCache MarkToMakeCaches(this ICharacterData characterData)
        {
            if (characterData == null)
                return null;
            if (!s_caches.ContainsKey(characterData))
            {
                // No stored caching data yet, create a new one and store to a colelction
                return s_caches[characterData] = new CharacterDataCache().MarkToMakeCaches();
            }
            return s_caches[characterData].MarkToMakeCaches();
        }

        public static void RemoveCaches(this ICharacterData characterData)
        {
            if (characterData == null)
                return;
            s_caches.Remove(characterData);
        }

        public static void Clear()
        {
            s_caches.Clear();
        }
    }
}
