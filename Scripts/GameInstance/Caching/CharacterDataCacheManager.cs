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
                s_caches[characterData] = new CharacterDataCache().MarkToMakeCaches().MakeCache(characterData);
            return s_caches[characterData].MakeCache(characterData);
        }

        public static CharacterDataCache MarkToMakeCaches(this ICharacterData characterData)
        {
            if (characterData == null)
                return null;
            if (!s_caches.ContainsKey(characterData))
                return new CharacterDataCache().MarkToMakeCaches();
            return s_caches[characterData].MarkToMakeCaches();
        }

        public static void Clear()
        {
            s_caches.Clear();
        }
    }
}
