using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class ItemExtension
    {
        #region Equipment Extension
        public static CharacterStats GetIncreaseStats(this Item equipmentItem, short level)
        {
            if (equipmentItem == null ||
                !equipmentItem.IsEquipment())
                return new CharacterStats();
            return equipmentItem.increaseStats.GetCharacterStats(level);
        }

        public static CharacterStats GetIncreaseStatsRate(this Item equipmentItem, short level)
        {
            if (equipmentItem == null ||
                !equipmentItem.IsEquipment())
                return new CharacterStats();
            return equipmentItem.increaseStatsRate.GetCharacterStats(level);
        }

        public static Dictionary<Attribute, float> GetIncreaseAttributes(this Item equipmentItem, short level)
        {
            Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
            if (equipmentItem != null &&
                equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineAttributes(equipmentItem.increaseAttributes, result, level, 1f);
            return result;
        }

        public static Dictionary<Attribute, float> GetIncreaseAttributesRate(this Item equipmentItem, short level)
        {
            Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
            if (equipmentItem != null &&
                equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineAttributes(equipmentItem.increaseAttributesRate, result, level, 1f);
            return result;
        }

        public static Dictionary<DamageElement, float> GetIncreaseResistances(this Item equipmentItem, short level)
        {
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            if (equipmentItem != null &&
                equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineResistances(equipmentItem.increaseResistances, result, level, 1f);
            return result;
        }

        public static Dictionary<DamageElement, float> GetIncreaseArmors(this Item equipmentItem, short level)
        {
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            if (equipmentItem != null &&
                equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineArmors(equipmentItem.increaseArmors, result, level, 1f);
            return result;
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages(this Item equipmentItem, short level)
        {
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            if (equipmentItem != null &&
                equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineDamages(equipmentItem.increaseDamages, result, level, 1f);
            return result;
        }

        public static Dictionary<BaseSkill, short> GetIncreaseSkills(this Item equipmentItem)
        {
            Dictionary<BaseSkill, short> result = new Dictionary<BaseSkill, short>();
            if (equipmentItem != null &&
                equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineSkills(equipmentItem.increaseSkillLevels, result);
            return result;
        }
        #endregion

        #region Armor/Shield Extension
        public static KeyValuePair<DamageElement, float> GetArmorAmount(this Item defendItem, short level, float rate)
        {
            if (defendItem == null ||
                !defendItem.IsDefendEquipment())
                return new KeyValuePair<DamageElement, float>();
            return GameDataHelpers.MakeArmor(defendItem.armorAmount, level, rate);
        }
        #endregion

        #region Weapon Extension
        public static KeyValuePair<DamageElement, MinMaxFloat> GetDamageAmount(this Item weaponItem, short itemLevel, float statsRate, ICharacterData character)
        {
            if (weaponItem == null ||
                !weaponItem.IsWeapon())
                return new KeyValuePair<DamageElement, MinMaxFloat>();
            return GameDataHelpers.MakeDamage(weaponItem.damageAmount, itemLevel, statsRate, weaponItem.GetEffectivenessDamage(character));
        }

        public static float GetEffectivenessDamage(this Item weaponItem, ICharacterData character)
        {
            if (weaponItem == null ||
                !weaponItem.IsWeapon() ||
                character == null)
                return 0f;
            return GameDataHelpers.GetEffectivenessDamage(weaponItem.WeaponType.CacheEffectivenessAttributes, character);
        }

        public static bool TryGetWeaponItemEquipType(this Item weaponItem, out WeaponItemEquipType equipType)
        {
            equipType = WeaponItemEquipType.OneHand;
            if (weaponItem == null ||
                !weaponItem.IsWeapon())
                return false;
            equipType = weaponItem.EquipType;
            return true;
        }
        #endregion
    }
}
