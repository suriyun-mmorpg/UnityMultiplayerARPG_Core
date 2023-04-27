using Newtonsoft.Json;
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

    public enum DualWieldRestriction : byte
    {
        None,
        MainHandRestricted,
        OffHandRestricted,
    }

    [CreateAssetMenu(fileName = GameDataMenuConsts.WEAPON_TYPE_FILE, menuName = GameDataMenuConsts.WEAPON_TYPE_MENU, order = GameDataMenuConsts.WEAPON_TYPE_ORDER)]
    public partial class WeaponType : BaseGameData
    {
        [Category("Weapon Type Settings")]
        [SerializeField]
        private WeaponItemEquipType equipType = WeaponItemEquipType.MainHandOnly;
        public WeaponItemEquipType EquipType { get { return equipType; } }
        [SerializeField]
        private DualWieldRestriction dualWieldRestriction = DualWieldRestriction.None;
        public DualWieldRestriction DualWieldRestriction { get { return dualWieldRestriction; } }
        [SerializeField]
        private DamageInfo damageInfo = default;
        public DamageInfo DamageInfo { get { return damageInfo; } }
        [SerializeField]
        private DamageEffectivenessAttribute[] effectivenessAttributes = new DamageEffectivenessAttribute[0];

        [Category("Ammo Settings")]
        [Tooltip("Require Ammo, Leave it to null when it is not required")]
        [SerializeField]
        private AmmoType requireAmmoType = null;
        public AmmoType RequireAmmoType { get { return requireAmmoType; } }

        [System.NonSerialized]
        private Dictionary<Attribute, float> _cacheEffectivenessAttributes;
        public Dictionary<Attribute, float> CacheEffectivenessAttributes
        {
            get
            {
                if (_cacheEffectivenessAttributes == null)
                    _cacheEffectivenessAttributes = GameDataHelpers.CombineDamageEffectivenessAttributes(effectivenessAttributes, new Dictionary<Attribute, float>());
                return _cacheEffectivenessAttributes;
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
