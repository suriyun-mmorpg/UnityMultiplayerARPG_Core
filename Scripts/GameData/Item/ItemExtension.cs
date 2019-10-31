using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class ItemExtension
    {
        #region Equipment Extension
        public static bool CanEquip(this Item equipmentItem, ICharacterData character, short level, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.None;
            if (equipmentItem == null ||
                !equipmentItem.IsEquipment() ||
                character == null)
                return false;

            // Check is it pass attribute requirement or not
            Dictionary<Attribute, float> attributeAmountsDict = character.GetAttributes(true, false);
            Dictionary<Attribute, float> requireAttributeAmounts = equipmentItem.CacheRequireAttributeAmounts;
            foreach (KeyValuePair<Attribute, float> requireAttributeAmount in requireAttributeAmounts)
            {
                if (!attributeAmountsDict.ContainsKey(requireAttributeAmount.Key) ||
                    attributeAmountsDict[requireAttributeAmount.Key] < requireAttributeAmount.Value)
                {
                    gameMessageType = GameMessage.Type.NotEnoughAttributeAmounts;
                    return false;
                }
            }

            // Check another requirements
            if (equipmentItem.requirement.character != null && equipmentItem.requirement.character != character.GetDatabase())
            {
                gameMessageType = GameMessage.Type.NotMatchCharacterClass;
                return false;
            }

            if (character.Level < equipmentItem.requirement.level)
            {
                gameMessageType = GameMessage.Type.NotEnoughLevel;
                return false;
            }

            return true;
        }

        public static bool CanAttack(this Item weaponItem, ICharacterData character)
        {
            if (weaponItem == null ||
                !weaponItem.IsWeapon() ||
                character == null)
                return false;

            AmmoType requireAmmoType = weaponItem.WeaponType.requireAmmoType;
            return requireAmmoType == null || character.IndexOfAmmoItem(requireAmmoType) >= 0;
        }

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
