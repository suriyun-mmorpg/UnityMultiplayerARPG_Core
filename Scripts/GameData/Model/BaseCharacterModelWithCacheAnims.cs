using System.Collections.Generic;

namespace MultiplayerARPG
{
    /// <summary>
    /// An Animation data will be cached here in static dictionaries
    /// so extending class will use data from the static dictionaries
    /// so cached dictionaries will not re-create 
    /// when new character model (which extend from this class) instantiates
    /// </summary>
    /// <typeparam name="TWeaponAnims"></typeparam>
    /// <typeparam name="TSkillAnims"></typeparam>
    public abstract class BaseCharacterModelWithCacheAnims<TWeaponAnims, TSkillAnims> : BaseCharacterModel
        where TWeaponAnims : IWeaponAnims
        where TSkillAnims : ISkillAnims
    {
        protected static readonly Dictionary<int, CacheAnimations> allCacheAnims = new Dictionary<int, CacheAnimations>();
        
        private CacheAnimations CacheAnims
        {
            get
            {
                int instanceId = gameObject.GetInstanceID();
                if (!allCacheAnims.ContainsKey(instanceId))
                    allCacheAnims.Add(instanceId, new CacheAnimations(GetWeaponAnims(), GetSkillAnims()));
                return allCacheAnims[instanceId];
            }
        }
        
        protected CacheAnimations GetAnims()
        {
            return CacheAnims;
        }

        protected abstract TWeaponAnims[] GetWeaponAnims();
        protected abstract TSkillAnims[] GetSkillAnims();

        protected class CacheAnimations
        {
            public Dictionary<int, TWeaponAnims> CacheWeaponAnimations { get; private set; }
            public Dictionary<int, TSkillAnims> CacheSkillAnimations { get; private set; }

            public CacheAnimations(TWeaponAnims[] weaponAnimations, TSkillAnims[] skillAnimations)
            {
                CacheWeaponAnimations = new Dictionary<int, TWeaponAnims>();
                foreach (TWeaponAnims weaponAnimation in weaponAnimations)
                {
                    if (weaponAnimation.Data == null) continue;
                    CacheWeaponAnimations[weaponAnimation.Data.DataId] = weaponAnimation;
                }

                CacheSkillAnimations = new Dictionary<int, TSkillAnims>();
                foreach (TSkillAnims skillAnimation in skillAnimations)
                {
                    if (skillAnimation.Data == null) continue;
                    CacheSkillAnimations[skillAnimation.Data.DataId] = skillAnimation;
                }
            }
        }
    }
}
