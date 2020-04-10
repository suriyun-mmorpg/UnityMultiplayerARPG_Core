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
        [SerializeField]
        private WeaponItemEquipType equipType;
        public WeaponItemEquipType EquipType { get { return equipType; } }
        [SerializeField]
        private DamageInfo damageInfo;
        public DamageInfo DamageInfo { get { return damageInfo; } }
        [SerializeField]
        private DamageEffectivenessAttribute[] effectivenessAttributes;
        [Header("Ammo")]
        [Tooltip("Require Ammo, Leave it to null when it is not required")]
        [SerializeField]
        private AmmoType requireAmmoType;
        public AmmoType RequireAmmoType { get { return requireAmmoType; } }

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

        public WeaponType GenerateDefaultWeaponType()
        {
            name = GameDataConst.UNKNOW_WEAPON_TYPE_ID;
            title = GameDataConst.UNKNOW_WEAPON_TYPE_TITLE;
            equipType = WeaponItemEquipType.OneHand;
            damageInfo = new DamageInfo();
            effectivenessAttributes = new DamageEffectivenessAttribute[0];
            requireAmmoType = null;
            return this;
        }
    }
}
