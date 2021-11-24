using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum WeaponItemEquipType : byte
    {
        MainHandOnly,
        DualWieldable,
        TwoHand,
        OffHandOnly,
    }

    [CreateAssetMenu(fileName = "Weapon Type", menuName = "Create GameData/Weapon Type", order = -4895)]
    public partial class WeaponType : BaseGameData
    {
        [Category("Weapon Type Settings")]
        [SerializeField]
        private WeaponItemEquipType equipType = WeaponItemEquipType.MainHandOnly;
        public WeaponItemEquipType EquipType { get { return equipType; } }
        [SerializeField]
        private DamageInfo damageInfo = default(DamageInfo);
        public DamageInfo DamageInfo { get { return damageInfo; } }
        [SerializeField]
        private DamageEffectivenessAttribute[] effectivenessAttributes = new DamageEffectivenessAttribute[0];

        [Category("Ammo Settings")]
        [Tooltip("Require Ammo, Leave it to null when it is not required")]
        [SerializeField]
        private AmmoType requireAmmoType = null;
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
            defaultTitle = GameDataConst.UNKNOW_WEAPON_TYPE_TITLE;
            return this;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddAmmoTypes(RequireAmmoType);
        }
    }
}
