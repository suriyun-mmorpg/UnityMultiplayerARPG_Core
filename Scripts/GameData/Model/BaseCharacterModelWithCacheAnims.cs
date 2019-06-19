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
    /// <typeparam name="TVehicleAnims"></typeparam>
    public abstract class BaseCharacterModelWithCacheAnims<TWeaponAnims, TSkillAnims, TVehicleAnims> : BaseCharacterModel
        where TWeaponAnims : IWeaponAnims
        where TSkillAnims : ISkillAnims
        where TVehicleAnims : IVehicleAnims<TWeaponAnims, TSkillAnims>
    {
        protected static readonly Dictionary<int, CacheAnimations> allCacheAnims = new Dictionary<int, CacheAnimations>();
        protected static readonly Dictionary<int, Dictionary<int, CacheAnimations>> allCacheVehicleAnims = new Dictionary<int, Dictionary<int, CacheAnimations>>();
        
        private CacheAnimations CacheAnims
        {
            get
            {
                if (!allCacheAnims.ContainsKey(DataId))
                    allCacheAnims.Add(DataId, new CacheAnimations(GetWeaponAnims(), GetSkillAnims()));
                return allCacheAnims[DataId];
            }
        }

        private Dictionary<int, CacheAnimations> CacheVehicleAnims
        {
            get
            {
                if (!allCacheVehicleAnims.ContainsKey(DataId))
                {
                    Dictionary<int, CacheAnimations> vehicleAnims = new Dictionary<int, CacheAnimations>();
                    foreach (IVehicleAnims<TWeaponAnims, TSkillAnims> vehicleAnimation in GetVehicleAnims())
                    {
                        if (vehicleAnimation.Data == null) continue;
                        vehicleAnims[vehicleAnimation.Data.DataId] = new CacheAnimations(vehicleAnimation.WeaponAnims, vehicleAnimation.SkillAnims);
                    }
                    allCacheVehicleAnims.Add(DataId, vehicleAnims);
                }
                return allCacheVehicleAnims[DataId];
            }
        }

        private int vehicleDataId;
        protected CacheAnimations GetAnims()
        {
            if (CacheVehicleAnims.ContainsKey(vehicleDataId))
                return CacheVehicleAnims[vehicleDataId];
            return CacheAnims;
        }

        protected abstract TWeaponAnims[] GetWeaponAnims();
        protected abstract TSkillAnims[] GetSkillAnims();
        protected abstract TVehicleAnims[] GetVehicleAnims();

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
