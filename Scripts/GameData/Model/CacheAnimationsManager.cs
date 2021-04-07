using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class CacheAnimationsManager
    {
        private static readonly Dictionary<int, ICacheAnimations> CacheAnims = new Dictionary<int, ICacheAnimations>();

        public static void SetCacheAnimations<TWeaponAnims, TSkillAnims>(int hashAssetId, IEnumerable<TWeaponAnims> weaponAnimations, IEnumerable<TSkillAnims> skillAnimations)
            where TWeaponAnims : IWeaponAnims
            where TSkillAnims : ISkillAnims
        {
            if (CacheAnims.ContainsKey(hashAssetId)) return;
            CacheAnims[hashAssetId] = new CacheAnimations<TWeaponAnims, TSkillAnims>(weaponAnimations, skillAnimations);
        }

        public static CacheAnimations<TWeaponAnims, TSkillAnims> GetCacheAnimations<TWeaponAnims, TSkillAnims>(int hashAssetId)
            where TWeaponAnims : IWeaponAnims
            where TSkillAnims : ISkillAnims
        {
            return CacheAnims[hashAssetId] as CacheAnimations<TWeaponAnims, TSkillAnims>;
        }

        public static CacheAnimations<TWeaponAnims, TSkillAnims> SetAndGetCacheAnimations<TWeaponAnims, TSkillAnims>(int hashAssetId, IEnumerable<TWeaponAnims> weaponAnimations, IEnumerable<TSkillAnims> skillAnimations)
            where TWeaponAnims : IWeaponAnims
            where TSkillAnims : ISkillAnims
        {
            if (!CacheAnims.ContainsKey(hashAssetId))
                SetCacheAnimations(hashAssetId, weaponAnimations, skillAnimations);
            return GetCacheAnimations<TWeaponAnims, TSkillAnims>(hashAssetId);
        }

        public static bool SetAndTryGetCacheWeaponAnimations<TWeaponAnims, TSkillAnims>(int hashAssetId, IEnumerable<TWeaponAnims> weaponAnimations, IEnumerable<TSkillAnims> skillAnimations, int dataId, out TWeaponAnims anims)
            where TWeaponAnims : IWeaponAnims
            where TSkillAnims : ISkillAnims
        {
            if (!CacheAnims.ContainsKey(hashAssetId))
                SetCacheAnimations(hashAssetId, weaponAnimations, skillAnimations);
            return GetCacheAnimations<TWeaponAnims, TSkillAnims>(hashAssetId).CacheWeaponAnimations.TryGetValue(dataId, out anims);
        }

        public static bool SetAndTryGetCacheSkillAnimations<TWeaponAnims, TSkillAnims>(int hashAssetId, IEnumerable<TWeaponAnims> weaponAnimations, IEnumerable<TSkillAnims> skillAnimations, int dataId, out TSkillAnims anims)
            where TWeaponAnims : IWeaponAnims
            where TSkillAnims : ISkillAnims
        {
            if (!CacheAnims.ContainsKey(hashAssetId))
                SetCacheAnimations(hashAssetId, weaponAnimations, skillAnimations);
            return GetCacheAnimations<TWeaponAnims, TSkillAnims>(hashAssetId).CacheSkillAnimations.TryGetValue(dataId, out anims);
        }
    }
}
