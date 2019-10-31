using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UIBaseEquipmentBonus<T> : UISelectionEntry<T>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyHpStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_HP);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyMpStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MP);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyAccuracyStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ACCURACY);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyEvasionStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_EVASION);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyCriRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRITICAL_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyCriDmgRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRITICAL_DAMAGE_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyBlockRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BLOCK_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyBlockDmgRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BLOCK_DAMAGE_RATE);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyMoveSpeedStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MOVE_SPEED);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyAtkSpeedStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTACK_SPEED);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyWeightLimitStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_WEIGHT);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeySlotLimitStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SLOT);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyStaminaStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_STAMINA);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyFoodStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_FOOD);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyWaterStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_WATER);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyHpRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_HP_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyMpRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MP_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyAccuracyRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ACCURACY_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyEvasionRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_EVASION_RATE);
        [Tooltip("Format => {0} = {Amount * 10000}")]
        public UILocaleKeySetting formatKeyCriRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRITICAL_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 10000}")]
        public UILocaleKeySetting formatKeyCriDmgRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRITICAL_DAMAGE_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 10000}")]
        public UILocaleKeySetting formatKeyBlockRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BLOCK_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 10000}")]
        public UILocaleKeySetting formatKeyBlockDmgRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BLOCK_DAMAGE_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyMoveSpeedRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MOVE_SPEED_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyAtkSpeedRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTACK_SPEED_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyWeightLimitRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_WEIGHT_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeySlotLimitRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SLOT_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyStaminaRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_STAMINA_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyFoodRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_FOOD_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyWaterRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_WATER_RATE);
        [Tooltip("Format => {0} = {Attribute Title}, {1} = {Amount}")]
        public UILocaleKeySetting formatKeyAttributeAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTRIBUTE_AMOUNT);
        [Tooltip("Format => {0} = {Attribute Title}, {1} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyAttributeAmountRate = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTRIBUTE_RATE);
        [Tooltip("Format => {0} = {Damage Element Title}, {1} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyResistanceAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_RESISTANCE_AMOUNT);
        [Tooltip("Format => {0} = {Damage Element Title}, {1} = {Min Damage}, {2} = {Max Damage}")]
        public UILocaleKeySetting formatKeyDamageAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_DAMAGE_WITH_ELEMENTAL);
        [Tooltip("Format => {0} = {Damage Element Title}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyArmorAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ARMOR_AMOUNT);
        [Tooltip("Format => {0} = {Skill Title}, {1} = {Level}")]
        public UILocaleKeySetting formatKeySkillLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_LEVEL);

        [Header("UI Elements")]
        public TextWrapper uiTextAllBonus;

        public string GetEquipmentBonusText(EquipmentBonus equipmentBonus)
        {
            string result = string.Empty;

            string statsText = CharacterStats.GetText(
                equipmentBonus.stats,
                false,
                true,
                formatKeyHpStats,
                formatKeyMpStats,
                formatKeyAccuracyStats,
                formatKeyEvasionStats,
                formatKeyCriRateStats,
                formatKeyCriDmgRateStats,
                formatKeyBlockRateStats,
                formatKeyBlockDmgRateStats,
                formatKeyMoveSpeedStats,
                formatKeyAtkSpeedStats,
                formatKeyWeightLimitStats,
                formatKeySlotLimitStats,
                formatKeyStaminaStats,
                formatKeyFoodStats,
                formatKeyWaterStats);

            string statsRateText = CharacterStats.GetText(
                equipmentBonus.statsRate,
                true,
                true,
                formatKeyHpRateStats,
                formatKeyMpRateStats,
                formatKeyAccuracyRateStats,
                formatKeyEvasionRateStats,
                formatKeyCriRateRateStats,
                formatKeyCriDmgRateRateStats,
                formatKeyBlockRateRateStats,
                formatKeyBlockDmgRateRateStats,
                formatKeyMoveSpeedRateStats,
                formatKeyAtkSpeedRateStats,
                formatKeyWeightLimitRateStats,
                formatKeySlotLimitRateStats,
                formatKeyStaminaRateStats,
                formatKeyFoodRateStats,
                formatKeyWaterRateStats);

            if (!string.IsNullOrEmpty(statsText))
                result += statsText;

            if (!string.IsNullOrEmpty(statsRateText))
            {
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += statsRateText;
            }

            // Attributes
            foreach (AttributeAmount entry in equipmentBonus.attributes)
            {
                if (entry.attribute == null || entry.amount == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(
                    LanguageManager.GetText(formatKeyAttributeAmount),
                    entry.attribute.Title,
                    entry.amount.ToBonusString());
            }
            foreach (AttributeAmount entry in equipmentBonus.attributesRate)
            {
                if (entry.attribute == null || entry.amount == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(
                    LanguageManager.GetText(formatKeyAttributeAmountRate),
                    entry.attribute.Title,
                    (entry.amount * 100).ToBonusString());
            }

            DamageElement tempElement;
            // Resistances
            foreach (ResistanceAmount entry in equipmentBonus.resistances)
            {
                if (entry.amount == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                tempElement = entry.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : entry.damageElement;
                result += string.Format(
                    LanguageManager.GetText(formatKeyResistanceAmount),
                    tempElement.Title,
                    (entry.amount * 100).ToBonusString());
            }

            // Damages
            foreach (DamageAmount entry in equipmentBonus.damages)
            {
                if (entry.amount.min == 0 && entry.amount.max == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                tempElement = entry.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : entry.damageElement;
                result += string.Format(
                    LanguageManager.GetText(formatKeyDamageAmount),
                    tempElement.Title,
                    entry.amount.min.ToBonusString(),
                    entry.amount.max.ToBonusString());
            }

            // Armors
            foreach (ArmorAmount entry in equipmentBonus.armors)
            {
                if (entry.amount == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                tempElement = entry.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : entry.damageElement;
                result += string.Format(
                    LanguageManager.GetText(formatKeyArmorAmount),
                    tempElement.Title,
                    entry.amount.ToBonusString());
            }

            // Skills
            foreach (SkillLevel entry in equipmentBonus.skills)
            {
                if (entry.skill == null || entry.level == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(
                    LanguageManager.GetText(formatKeySkillLevel),
                    entry.skill.Title,
                    entry.level.ToBonusString());
            }

            return result;
        }
    }
}
