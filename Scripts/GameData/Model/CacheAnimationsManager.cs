using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class CacheAnimationsManager
    {
        private static readonly Dictionary<string, ICacheAnimations> CacheAnims = new Dictionary<string, ICacheAnimations>();

        public static void SetCacheAnimations<TWeaponAnims, TSkillAnims>(string id, IEnumerable<TWeaponAnims> weaponAnimations, IEnumerable<TSkillAnims> skillAnimations)
            where TWeaponAnims : IWeaponAnims
            where TSkillAnims : ISkillAnims
        {
            if (CacheAnims.ContainsKey(id)) return;
            CacheAnims[id] = new CacheAnimations<TWeaponAnims, TSkillAnims>(weaponAnimations, skillAnimations);
        }

        public static CacheAnimations<TWeaponAnims, TSkillAnims> GetCacheAnimations<TWeaponAnims, TSkillAnims>(string id)
            where TWeaponAnims : IWeaponAnims
            where TSkillAnims : ISkillAnims
        {
            return CacheAnims[id] as CacheAnimations<TWeaponAnims, TSkillAnims>;
        }

        public static CacheAnimations<TWeaponAnims, TSkillAnims> SetAndGetCacheAnimations<TWeaponAnims, TSkillAnims>(string id, IEnumerable<TWeaponAnims> weaponAnimations, IEnumerable<TSkillAnims> skillAnimations)
            where TWeaponAnims : IWeaponAnims
            where TSkillAnims : ISkillAnims
        {
            if (!CacheAnims.ContainsKey(id))
                SetCacheAnimations(id, weaponAnimations, skillAnimations);
            return GetCacheAnimations<TWeaponAnims, TSkillAnims>(id);
        }

        public static bool SetAndTryGetCacheWeaponAnimations<TWeaponAnims, TSkillAnims>(string id, IEnumerable<TWeaponAnims> weaponAnimations, IEnumerable<TSkillAnims> skillAnimations, int dataId, out TWeaponAnims anims)
            where TWeaponAnims : IWeaponAnims
            where TSkillAnims : ISkillAnims
        {
            if (!CacheAnims.ContainsKey(id))
                SetCacheAnimations(id, weaponAnimations, skillAnimations);
            return GetCacheAnimations<TWeaponAnims, TSkillAnims>(id).CacheWeaponAnimations.TryGetValue(dataId, out anims);
        }

        public static bool SetAndTryGetCacheSkillAnimations<TWeaponAnims, TSkillAnims>(string id, IEnumerable<TWeaponAnims> weaponAnimations, IEnumerable<TSkillAnims> skillAnimations, int dataId, out TSkillAnims anims)
            where TWeaponAnims : IWeaponAnims
            where TSkillAnims : ISkillAnims
        {
            if (!CacheAnims.ContainsKey(id))
                SetCacheAnimations(id, weaponAnimations, skillAnimations);
            return GetCacheAnimations<TWeaponAnims, TSkillAnims>(id).CacheSkillAnimations.TryGetValue(dataId, out anims);
        }
    }
}
