using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class CharacterDataCacheManager
    {
        private static readonly Dictionary<ICharacterData, CharacterDataCache> caches = new Dictionary<ICharacterData, CharacterDataCache>();

        public static CharacterDataCache GetCaches(this ICharacterData characterData)
        {
            if (characterData == null)
                return null;
            if (!caches.ContainsKey(characterData))
                caches[characterData] = new CharacterDataCache().MarkToMakeCaches().MakeCache(characterData);
            return caches[characterData].MakeCache(characterData);
        }

        public static CharacterDataCache MarkToMakeCaches(this ICharacterData characterData)
        {
            if (characterData == null)
                return null;
            if (!caches.ContainsKey(characterData))
                return new CharacterDataCache().MarkToMakeCaches();
            return caches[characterData].MarkToMakeCaches();
        }

        public static void Clear()
        {
            caches.Clear();
        }
    }
}
