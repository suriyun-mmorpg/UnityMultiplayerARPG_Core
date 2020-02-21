using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class ItemExtension
    {
        #region Ammo Extension
        public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages(this IAmmoItem ammoItem, short level)
        {
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            if (ammoItem != null && ammoItem.IsAmmo())
                result = GameDataHelpers.CombineDamages(ammoItem.IncreaseDamages, result, level, 1f);
            return result;
        }
        #endregion

        #region Equipment Extension
        public static CharacterStats GetIncreaseStats<T>(this T equipmentItem, short level)
            where T : IEquipmentItem
        {
            if (equipmentItem == null || !equipmentItem.IsEquipment())
                return new CharacterStats();
            return equipmentItem.IncreaseStats.GetCharacterStats(level);
        }

        public static CharacterStats GetIncreaseStatsRate<T>(this T equipmentItem, short level)
            where T : IEquipmentItem
        {
            if (equipmentItem == null || !equipmentItem.IsEquipment())
                return new CharacterStats();
            return equipmentItem.IncreaseStatsRate.GetCharacterStats(level);
        }

        public static Dictionary<Attribute, float> GetIncreaseAttributes<T>(this T equipmentItem, short level)
            where T : IEquipmentItem
        {
            Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineAttributes(equipmentItem.IncreaseAttributes, result, level, 1f);
            return result;
        }

        public static Dictionary<Attribute, float> GetIncreaseAttributesRate<T>(this T equipmentItem, short level)
            where T : IEquipmentItem
        {
            Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineAttributes(equipmentItem.IncreaseAttributesRate, result, level, 1f);
            return result;
        }

        public static Dictionary<DamageElement, float> GetIncreaseResistances<T>(this T equipmentItem, short level)
            where T : IEquipmentItem
        {
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineResistances(equipmentItem.IncreaseResistances, result, level, 1f);
            return result;
        }

        public static Dictionary<DamageElement, float> GetIncreaseArmors<T>(this T equipmentItem, short level)
            where T : IEquipmentItem
        {
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineArmors(equipmentItem.IncreaseArmors, result, level, 1f);
            return result;
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages<T>(this T equipmentItem, short level)
            where T : IEquipmentItem
        {
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineDamages(equipmentItem.IncreaseDamages, result, level, 1f);
            return result;
        }

        public static Dictionary<BaseSkill, short> GetIncreaseSkills<T>(this T equipmentItem)
            where T : IEquipmentItem
        {
            Dictionary<BaseSkill, short> result = new Dictionary<BaseSkill, short>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineSkills(equipmentItem.IncreaseSkillLevels, result);
            return result;
        }
        #endregion

        #region Armor/Shield Extension
        public static KeyValuePair<DamageElement, float> GetArmorAmount<T>(this T defendItem, short level, float rate)
            where T : IDefendEquipmentItem
        {
            if (defendItem == null || !defendItem.IsDefendEquipment())
                return new KeyValuePair<DamageElement, float>();
            return GameDataHelpers.MakeArmor(defendItem.ArmorAmount, level, rate);
        }
        #endregion

        #region Weapon Extension
        public static KeyValuePair<DamageElement, MinMaxFloat> GetDamageAmount<T>(this T weaponItem, short itemLevel, float statsRate, ICharacterData character)
            where T : IWeaponItem
        {
            if (weaponItem == null || !weaponItem.IsWeapon())
                return new KeyValuePair<DamageElement, MinMaxFloat>();
            return GameDataHelpers.MakeDamage(weaponItem.DamageAmount, itemLevel, statsRate, weaponItem.GetEffectivenessDamage(character));
        }

        public static float GetEffectivenessDamage<T>(this T weaponItem, ICharacterData character)
            where T : IWeaponItem
        {
            if (weaponItem == null || !weaponItem.IsWeapon())
                return 0f;
            return GameDataHelpers.GetEffectivenessDamage(weaponItem.WeaponType.CacheEffectivenessAttributes, character);
        }

        public static bool TryGetWeaponItemEquipType<T>(this T weaponItem, out WeaponItemEquipType equipType)
            where T : IWeaponItem
        {
            equipType = WeaponItemEquipType.OneHand;
            if (weaponItem == null || !weaponItem.IsWeapon())
                return false;
            equipType = weaponItem.EquipType;
            return true;
        }
        #endregion
    }
}
