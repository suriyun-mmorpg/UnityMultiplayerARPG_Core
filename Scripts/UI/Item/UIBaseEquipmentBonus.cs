using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UIBaseEquipmentBonus<T> : UISelectionEntry<T>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyHpStats = UILocaleKeys.UI_FORMAT_HP.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyMpStats = UILocaleKeys.UI_FORMAT_MP.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyArmorStats = UILocaleKeys.UI_FORMAT_ARMOR.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyAccuracyStats = UILocaleKeys.UI_FORMAT_ACCURACY.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyEvasionStats = UILocaleKeys.UI_FORMAT_EVASION.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyCriRateStats = UILocaleKeys.UI_FORMAT_CRITICAL_RATE.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyCriDmgRateStats = UILocaleKeys.UI_FORMAT_CRITICAL_DAMAGE_RATE.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyBlockRateStats = UILocaleKeys.UI_FORMAT_BLOCK_RATE.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyBlockDmgRateStats = UILocaleKeys.UI_FORMAT_BLOCK_DAMAGE_RATE.ToString();
        [Tooltip("Format => {0} = {Move Speed}")]
        public string formatKeyMoveSpeedStats = UILocaleKeys.UI_FORMAT_MOVE_SPEED.ToString();
        [Tooltip("Format => {0} = {Attack Speed}")]
        public string formatKeyAtkSpeedStats = UILocaleKeys.UI_FORMAT_ATTACK_SPEED.ToString();
        [Tooltip("Format => {0} = {Weight Limit}")]
        public string formatKeyWeightLimitStats = UILocaleKeys.UI_FORMAT_WEIGHT.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyStaminaStats = UILocaleKeys.UI_FORMAT_STAMINA.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyFoodStats = UILocaleKeys.UI_FORMAT_FOOD.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyWaterStats = UILocaleKeys.UI_FORMAT_WATER.ToString();
        [Tooltip("Format => {0} = {Attribute Title}, {1} = {Amount}")]
        public string formatKeyAttributeAmount = UILocaleKeys.UI_FORMAT_ATTRIBUTE_AMOUNT.ToString();
        [Tooltip("Format => {0} = {Damage Element Title}, {1} = {Amount * 100}")]
        public string formatKeyResistanceAmount = UILocaleKeys.UI_FORMAT_RESISTANCE_AMOUNT.ToString();
        [Tooltip("Format => {0} = {Damage Element Title}, {1} = {Min Damage}, {2} = {Max Damage}")]
        public string formatKeyDamageAmount = UILocaleKeys.UI_FORMAT_DAMAGE_WITH_ELEMENTAL.ToString();
        [Tooltip("Format => {0} = {Skill Title}, {1} = {Level}")]
        public string formatKeySkillLevel = UILocaleKeys.UI_FORMAT_SKILL_LEVEL.ToString();

        [Header("UI Elements")]
        public TextWrapper uiTextAllBonus;

        public string GetEquipmentBonusText(EquipmentBonus equipmentBonus)
        {
            string result = CharacterStats.GetText(
                equipmentBonus.stats,
                formatKeyHpStats,
                formatKeyMpStats,
                formatKeyArmorStats,
                formatKeyAccuracyStats,
                formatKeyEvasionStats,
                formatKeyCriRateStats,
                formatKeyCriDmgRateStats,
                formatKeyBlockRateStats,
                formatKeyBlockDmgRateStats,
                formatKeyMoveSpeedStats,
                formatKeyAtkSpeedStats,
                formatKeyWeightLimitStats,
                formatKeyStaminaStats,
                formatKeyFoodStats,
                formatKeyWaterStats);

            foreach (AttributeAmount entry in equipmentBonus.attributes)
            {
                if (entry.attribute == null || entry.amount == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(
                    LanguageManager.GetText(formatKeyAttributeAmount),
                    entry.attribute.Title,
                    entry.amount.ToString("N0"));
            }
            foreach (ResistanceAmount entry in equipmentBonus.resistances)
            {
                if (entry.damageElement == null || entry.amount == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(
                    LanguageManager.GetText(formatKeyResistanceAmount),
                    entry.damageElement.Title,
                    (entry.amount * 100).ToString("N0"));
            }
            foreach (DamageAmount entry in equipmentBonus.damages)
            {
                if (entry.damageElement == null || (entry.amount.min == 0 && entry.amount.max == 0))
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(
                    LanguageManager.GetText(formatKeyDamageAmount),
                    entry.damageElement.Title,
                    entry.amount.min.ToString("N0"),
                    entry.amount.max.ToString("N0"));
            }
            foreach (SkillLevel entry in equipmentBonus.skills)
            {
                if (entry.skill == null || entry.level == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(
                    LanguageManager.GetText(formatKeySkillLevel),
                    entry.skill.Title,
                    entry.level.ToString("N0"));
            }
            return result;
        }
    }
}
