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

    public enum FireType : byte
    {
        SingleFire,
        Automatic,
    }

    [CreateAssetMenu(fileName = "Weapon Type", menuName = "Create GameData/Weapon Type")]
    public partial class WeaponType : BaseGameData
    {
        public WeaponItemEquipType equipType;
        public DamageInfo damageInfo;
        public DamageEffectivenessAttribute[] effectivenessAttributes;
        public BaseWeaponAbility weaponAbility;
        public CrosshairSetting crosshairSetting = new CrosshairSetting()
        {
            expandPerFrameWhileMoving = 3f,
            expandPerFrameWhileAttacking = 5f,
            shrinkPerFrame = 8f,
            minSpread = 10f,
            maxSpread = 50f
        };
        [Header("Fire Options")]
        public FireType fireType;
        public Vector2 fireStagger;
        public byte fireSpread;
        [Header("Ammo")]
        [Tooltip("Require Ammo, Leave it to null when it is not required")]
        public AmmoType requireAmmoType;
        [Tooltip("For macine gun may set this to 30 as magazine capacity, if this is 0 it will not need to have ammo loaded to shoot but still need ammo in inventory")]
        public short ammoCapacity;

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

    [System.Serializable]
    public struct CrosshairSetting
    {
        public float expandPerFrameWhileMoving;
        public float expandPerFrameWhileAttacking;
        public float shrinkPerFrame;
        public float minSpread;
        public float maxSpread;
    }
}
