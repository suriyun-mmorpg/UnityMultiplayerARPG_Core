using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum WeaponItemEquipType : byte
    {
        OneHand,
        OneHandCanDual,
        TwoHand,
    }

    [CreateAssetMenu(fileName = "Weapon Type", menuName = "Create GameData/Weapon Type", order = -4895)]
    public partial class WeaponType : BaseGameData
    {
        [Header("Weapon Type Configs")]
        public WeaponItemEquipType equipType;
        public DamageInfo damageInfo;
        public DamageEffectivenessAttribute[] effectivenessAttributes;
        [Header("Ammo")]
        [Tooltip("Require Ammo, Leave it to null when it is not required")]
        public AmmoType requireAmmoType;

        [System.NonSerialized]
        private Dictionary<Attribute, float> cacheEffectivenessAttributes;
        public Dictionary<Attribute, float> CacheEffectivenessAttributes
        {
            get
            {
                if (cacheEffectivenessAttributes == null)
                    cacheEffectivenessAttributes = GameDataHelpers.CombineDamageEffectivenessAttributes(effectivenessAttributes, new Dictionary<Attribute, float>());
                return cacheEffectivenessAttributes;
            }
        }
    }
}
