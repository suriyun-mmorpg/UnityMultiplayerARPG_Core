using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UIBaseEquipmentBonus<T> : UISelectionEntry<T>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatHpStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatMpStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatArmorStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatAccuracyStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatEvasionStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatCriRateStats = "{0}: {1}%";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatCriDmgRateStats = "{0}: {1}%";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatBlockRateStats = "{0}: {1}%";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatBlockDmgRateStats = "{0}: {1}%";
        [Tooltip("Format => {0} = {Label}, {1} = {Move Speed}")]
        public string formatMoveSpeedStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Attack Speed}")]
        public string formatAtkSpeedStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Weight Limit}")]
        public string formatWeightLimitStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatStaminaStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatFoodStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatWaterStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Attribute Title}, {1} = {Amount}")]
        public string formatAttributeAmount = "{0}: {1}";
        [Tooltip("Format => {0} = {Element Title}, {1} = {Amount}")]
        public string formatResistanceAmount = "{0}: {1}";
        [Tooltip("Format => {0} = {Element Title}, {1} = {Min Damage}, {2} = {Max Damage}")]
        public string formatDamageAmount = "{0}: {1}~{2}";
        [Tooltip("Format => {0} = {Skill Title}, {1} = {Level}")]
        public string formatSkillLevel = "{0}: {1}";

        [Header("UI Elements")]
        public TextWrapper uiTextAllBonus;

        public string GetEquipmentBonusText(EquipmentBonus equipmentBonus)
        {
            string result = CharacterStats.GetText(
                equipmentBonus.stats,
                formatHpStats,
                formatMpStats,
                formatArmorStats,
                formatAccuracyStats,
                formatEvasionStats,
                formatCriRateStats,
                formatCriDmgRateStats,
                formatBlockRateStats,
                formatBlockDmgRateStats,
                formatMoveSpeedStats,
                formatAtkSpeedStats,
                formatWeightLimitStats,
                formatStaminaStats,
                formatFoodStats,
                formatWaterStats);
            foreach (AttributeAmount entry in equipmentBonus.attributes)
            {
                if (entry.attribute == null || entry.amount == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(formatAttributeAmount, entry.attribute.Title, entry.amount.ToString("N0"));
            }
            foreach (ResistanceAmount entry in equipmentBonus.resistances)
            {
                if (entry.damageElement == null || entry.amount == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(formatResistanceAmount, entry.damageElement.Title, (entry.amount * 100f).ToString("N0"));
            }
            foreach (DamageAmount entry in equipmentBonus.damages)
            {
                if (entry.damageElement == null || (entry.amount.min == 0 && entry.amount.max == 0))
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(formatDamageAmount, entry.damageElement.Title, entry.amount.min.ToString("N0"), entry.amount.max.ToString("N0"));
            }
            foreach (SkillLevel entry in equipmentBonus.skills)
            {
                if (entry.skill == null || entry.level == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(formatSkillLevel, entry.skill.Title, entry.level.ToString("N0"));
            }
            return result;
        }
    }
}
